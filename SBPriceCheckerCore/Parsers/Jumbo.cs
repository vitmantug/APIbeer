using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using NSoup;
using NSoup.Nodes;
using NSoup.Select;
using SBPriceCheckerCore.Helpers;
using SBPriceCheckerCore.Models;
using System.IO;

namespace SBPriceCheckerCore.Parsers
{
    public class Jumbo
    {
        private static string URL_SEARCH = "http://www.jumbo.pt/Frontoffice/ContentPages/CatalogSearch.aspx?Q=super%20bock";
        private static string URL_PRODUCT_DETAILS = "http://www.jumbo.pt/Frontoffice/ContentPages/CatalogProduct.aspx?id={0}";
        private static string URL_PRODUCT_IMAGE = "http://www.jumbo.pt/MediaServer/CatalogImages/Products/165_134/{0}_165_134.jpg";

        private Helper Helper = new Helper();

        public async Task<IEnumerable<Beer>> GetBeers()
        {
            List<Beer> _DbFromJumbo = new List<Beer>();

            try
            {
                HttpWebRequest webReq = (HttpWebRequest)HttpWebRequest.Create(URL_SEARCH);

                #region HttpWebRequest headers
                webReq.CookieContainer = new CookieContainer();
                webReq.Method = "GET";
                webReq.Host = "www.jumbo.pt";
                webReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                webReq.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.71 Safari/537.36";
                webReq.Headers.Add("Accept-Encoding", "gzip, deflate, sdch");
                webReq.Headers.Add("Accept-Language", "pt-PT,pt;q=0.8,en-US;q=0.6,en;q=0.4,es;q=0.2");
                webReq.Headers.Add("Upgrade-Insecure-Requests", "1");

                string detectSTS = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
                webReq.CookieContainer.Add(new Cookie("detectSTS", detectSTS) { Domain = webReq.Host }); //2015-10-16T13:51:13Z
                #endregion

                using (WebResponse response = await webReq.GetResponseAsync())
                {
                    Document webpageHtml = NSoupClient.Parse(response.GetResponseStream(), "utf-8");
                    Elements beersHtml = webpageHtml.Body.SiblingElements.Select("div.produtoGrelha");

                    foreach (Element beerHtml in beersHtml)
                    {
                        Beer beer = new Beer();
                        beer.store = "Jumbo";

                        #region parse id

                        beer.id = beerHtml.Attr("p");

                        #endregion

                        #region parse name

                        string name = beerHtml.GetElementsByClass("titProd").Text;
                        beer.name = name;

                        #endregion

                        #region parse total and capacity

                        string totalCapacity = beerHtml.GetElementsByClass("gr").Text.Replace("LT", "").Replace("L", "").Trim();
                        if (!String.IsNullOrEmpty(totalCapacity))
                        {
                            string total = string.Empty;
                            string capacity = string.Empty;

                            List<string> totalCapacityElems = totalCapacity.Split('X').ToList<string>();
                            int n = totalCapacityElems.Count;

                            switch (n)
                            {
                                case 1:
                                    beer.total = 1;

                                    capacity = totalCapacityElems.ElementAt(0);
                                    beer.capacity = Helper.ConvertPTNumberStrToDouble(capacity);
                                    break;

                                case 2:
                                    total = totalCapacityElems.ElementAt(0);
                                    beer.total = Convert.ToInt32(total);

                                    capacity = totalCapacityElems.ElementAt(1);
                                    beer.capacity = Helper.ConvertPTNumberStrToDouble(capacity);
                                    break;
                            }

                        }

                        #endregion

                        #region parse price

                        string priceHtml = beerHtml.GetElementsByClass("preco").Text.Trim();
                        if (!String.IsNullOrEmpty(priceHtml))
                        {
                            List<string> priceElems = priceHtml.Split('€').ToList<string>();
                            if (priceElems.Any())
                            {
                                double price = Helper.ConvertPTNumberStrToDouble(priceElems.ElementAt(0));

                                beer.priceBefore = Math.Round(price, 2, MidpointRounding.AwayFromZero);
                                beer.priceAfter = beer.priceBefore;
                            }
                        }

                        #endregion

                        #region parse price per litre

                        double pricePerLitre = beer.priceBefore / (beer.total * beer.capacity);
                        beer.pricePerLitre = Math.Round(pricePerLitre, 2, MidpointRounding.AwayFromZero);

                        #endregion

                        #region set beer urls

                        if (!String.IsNullOrEmpty(beer.id))
                        {
                            string idTxt = beer.id.PadLeft(8, '0');

                            beer.imageUrl = String.Format(URL_PRODUCT_IMAGE, idTxt);
                            beer.detailsUrl = String.Format(URL_PRODUCT_DETAILS, beer.id);
                        }

                        #endregion

                        _DbFromJumbo.Add(beer);
                    }
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
                return _DbFromJumbo.OrderBy(x => x.pricePerLitre);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return _DbFromJumbo.OrderBy(x => x.pricePerLitre);
            }

            return _DbFromJumbo.OrderBy(x => x.pricePerLitre);
        }
    }
}
