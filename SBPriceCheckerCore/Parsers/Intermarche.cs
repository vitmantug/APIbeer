using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSoup;
using NSoup.Nodes;
using NSoup.Select;
using SBPriceCheckerCore.Helpers;
using SBPriceCheckerCore.Models;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Threading;

namespace SBPriceCheckerCore.Parsers
{
    public class Intermarche
    {
        //http://stackoverflow.com/questions/22239357/how-to-cancel-task-await-after-a-timeout-period/22262976#22262976


        private static string STORE = "Intermarche";

        private static string URL_PRODUCT_DETAILS = "http://www.elcorteingles.pt/supermercado/sm2/pt_PT/520142/supermarket/bebidas/aguas-sumos-e-cervejas/cervejas/cerveja/{0}";

        private static string URL_LOGIN = "https://lojaonline.intermarche.pt/Accueil.aspx?p=16";
        private static string URL_SEARCH = "https://lojaonline.intermarche.pt/Catalogue/RechercheProduits.aspx?mot=super%20bock";

        private static Helper Helper = new Helper();

        static List<Beer> _DbFromIntermarche = new List<Beer>();

        public async Task<IEnumerable<Beer>> GetBeers()
        {
            try
            {
                if (Helper.PricesInCache(STORE))
                {
                    string dataJson = await Helper.ReadBeersRecordAsync(STORE).ConfigureAwait(false);

                    _DbFromIntermarche = JsonConvert.DeserializeObject<List<Beer>>(dataJson);
                }
                else
                {
                    var task = MessageLoopWorker.Run(DoWorkAsync, URL_LOGIN, URL_SEARCH);
                    task.Wait();
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
                return _DbFromIntermarche.OrderBy(x => x.pricePerLitre);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return _DbFromIntermarche.OrderBy(x => x.pricePerLitre);
            }

            return _DbFromIntermarche.OrderBy(x => x.pricePerLitre);
        }

        // navigate WebBrowser to the list of urls in a loop
        static async Task<object> DoWorkAsync(object[] args)
        {
            Console.WriteLine("Start working.");

            using (var wb = new WebBrowser())
            {
                wb.ScriptErrorsSuppressed = true;

                TaskCompletionSource<bool> tcs = null;
                WebBrowserDocumentCompletedEventHandler documentCompletedHandler = (s, e) => tcs.TrySetResult(true);

                // navigate to each URL in the list
                foreach (var url in args)
                {
                    tcs = new TaskCompletionSource<bool>();
                    wb.DocumentCompleted += documentCompletedHandler;
                    try
                    {
                        wb.Navigate(url.ToString());
                        // await for DocumentCompleted
                        await tcs.Task;
                    }
                    finally
                    {
                        wb.DocumentCompleted -= documentCompletedHandler;
                    }
                    // the DOM is ready
                    //Console.WriteLine(url.ToString());
                    //Console.WriteLine(wb.Document.Body.OuterHtml);

                    if (url.ToString().Equals(URL_SEARCH))
                    {
                        var documentElement = wb.Document.GetElementsByTagName("html")[0];

                        // poll the current HTML for changes asynchronosly
                        var html = documentElement.OuterHtml;
                        while (true)
                        {
                            // wait asynchronously, this will throw if cancellation requested
                            await Task.Delay(500);

                            // continue polling if the WebBrowser is still busy
                            if (wb.IsBusy)
                                continue;

                            var htmlNow = documentElement.OuterHtml;
                            if (html == htmlNow)
                                break; // no changes detected, end the poll loop

                            html = htmlNow;
                        }

                        #region parse html

                        Document webpageHtml = NSoupClient.Parse(html, "utf-8");
                        Element beersGridHtml = webpageHtml.Body.GetElementById("AffichageEnVignette");
                        Elements beersHtml = beersGridHtml.SiblingElements.Select("div.blocIteratifProduit");

                        foreach (Element beerHtml in beersHtml)
                        {
                            bool isSuperBock = false;

                            Beer beer = new Beer();
                            beer.store = STORE;

                            #region parse brand

                            string brand = beerHtml.GetElementsByClass("p_titre").First().Text();
                            if (!String.IsNullOrEmpty(brand) && brand.Equals("Super bock"))
                                isSuperBock = true;

                            #endregion

                            if (isSuperBock)
                            {
                                #region parse id

                                beer.id = beerHtml.Attr("data-id");

                                #endregion

                                _DbFromIntermarche.Add(beer);
                            }
                        }

                        if (_DbFromIntermarche.Any())
                            await Helper.InsertBeersRecordAsync(_DbFromIntermarche, STORE).ConfigureAwait(false);

                        #endregion

                    }
                }
            }

            Console.WriteLine("End working.");
            return null;
        }

    }

    // a helper class to start the message loop and execute an asynchronous task
    public static class MessageLoopWorker
    {
        public static async Task<object> Run(Func<object[], Task<object>> worker, params object[] args)
        {
            var tcs = new TaskCompletionSource<object>();

            var thread = new Thread(() =>
            {
                EventHandler idleHandler = null;

                idleHandler = async (s, e) =>
                {
                    // handle Application.Idle just once
                    Application.Idle -= idleHandler;

                    // return to the message loop
                    await Task.Yield();

                    // and continue asynchronously
                    // propogate the result or exception
                    try
                    {
                        var result = await worker(args);
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }

                    // signal to exit the message loop
                    // Application.Run will exit at this point
                    Application.ExitThread();
                };

                // handle Application.Idle just once
                // to make sure we're inside the message loop
                // and SynchronizationContext has been correctly installed
                Application.Idle += idleHandler;
                Application.Run();
            });

            // set STA model for the new thread
            thread.SetApartmentState(ApartmentState.STA);

            // start the thread and await for the task
            thread.Start();
            try
            {
                return await tcs.Task;
            }
            finally
            {
                thread.Join();
            }
        }
    }
}
