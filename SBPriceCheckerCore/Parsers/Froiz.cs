using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SBPriceCheckerCore.Models;
using SBPriceCheckerCore.Helpers;
using NSoup;
using NSoup.Nodes;
using NSoup.Select;
using System.Net;
using System.Globalization;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace SBPriceCheckerCore.Parsers
{
    public class Froiz
    {
        private static string STORE = "Froiz";

        private static string URL_PRODUCT_IMAGE = "https://loja.froiz.com/fotopro/thumbnails/{0}.jpg";
        private static string URL_PRODUCT_DETAILS = "https://loja.froiz.com/product/{0}/";
        private static string URL_SEARCH = "https://loja.froiz.com/search.php?q=super+bock";

        private Helper Helper = new Helper();

        public async Task<IEnumerable<Beer>> GetBeers()
        {
            List<Beer> _DbFromFroiz = new List<Beer>();

            try
            {
                if (Helper.PricesInCache(STORE))
                {
                    string dataJson = await Helper.ReadBeersRecordAsync(STORE).ConfigureAwait(false);

                    _DbFromFroiz = JsonConvert.DeserializeObject<List<Beer>>(dataJson);
                }
                else
                {
                    Document webpageHtml = NSoupClient.Parse(await new WebClient().OpenReadTaskAsync(URL_SEARCH).ConfigureAwait(false), "UTF-8");

                    Elements beersHtml = webpageHtml.Body.SiblingElements.Select("div.isotope--target");

                    foreach (Element beerHtml in beersHtml)
                    {
                        Beer beer = new Beer();
                        beer.store = STORE;

                        #region parse Id

                        Elements idHtml = beerHtml.GetElementsByAttributeValue("name", "id");
                        if (idHtml.Any())
                        {
                            beer.id = idHtml.First().Attr("value");
                        }

                        #endregion

                        #region parse name

                        Elements nameHtml = beerHtml.GetElementsByClass("dproducto");
                        if (nameHtml.Any())
                        {
                            beer.name = nameHtml.First().Text();
                        }

                        #endregion

                        #region parse total and capacity

                        bool splitByX = false;

                        if (!String.IsNullOrEmpty(beer.name))
                        {
                            List<string> nameValues = beer.name.Split(' ').ToList();
                            if (nameValues.Any())
                            {
                                string totalCapacity = nameValues.Where(x => x.Contains("x") && x.Contains("cl")).FirstOrDefault();
                                if (String.IsNullOrEmpty(totalCapacity))
                                {
                                    totalCapacity = nameValues.Where(x => x.Contains("cl")).FirstOrDefault();
                                    if (String.IsNullOrEmpty(totalCapacity))
                                    {
                                        totalCapacity = nameValues.Where(x => x.Contains("litro")).FirstOrDefault();
                                        if (String.IsNullOrEmpty(totalCapacity))
                                        {
                                            totalCapacity = nameValues.Where(x => x.Contains("x")).FirstOrDefault();
                                            if (String.IsNullOrEmpty(totalCapacity))
                                            {
                                                //...
                                            }
                                            else
                                            {
                                                splitByX = true;
                                            }
                                        }
                                        else
                                        {
                                            beer.capacity = 1;
                                            beer.total = 1;
                                        }
                                    }
                                    else
                                    {
                                        int index = nameValues.FindIndex(x => x.Contains("Leve"));
                                        if (index == -1)
                                        {
                                            string totalCapacityAux = nameValues.Where(x => x.Contains("x")).FirstOrDefault();
                                            if (String.IsNullOrEmpty(totalCapacityAux))
                                            {
                                                beer.total = 1;

                                                string capacity = totalCapacity.Replace("cl", "");
                                                beer.capacity = Helper.ConvertPTNumberStrToDouble("0," + capacity);
                                            }
                                            else
                                            {
                                                totalCapacity = totalCapacityAux;
                                                splitByX = true;
                                            }
                                        }
                                        else
                                        {
                                            string total = nameValues.ElementAt(index + 1);
                                            beer.total = Convert.ToInt32(total);

                                            string capacity = totalCapacity.Replace("cl", "");
                                            beer.capacity = Helper.ConvertPTNumberStrToDouble("0," + capacity);
                                        }
                                    }
                                }
                                else
                                {
                                    splitByX = true;
                                }

                                if (splitByX)
                                {
                                    string total = string.Empty;
                                    string capacity = string.Empty;

                                    List<string> tcValues = totalCapacity.Split('x').ToList();
                                    if (tcValues.Any() && tcValues.Count == 2)
                                    {
                                        total = tcValues.ElementAt(0);
                                        beer.total = Convert.ToInt32(total);

                                        if (tcValues.ElementAt(1).Contains("cl"))
                                        {
                                            capacity = tcValues.ElementAt(1).Replace("cl", "");
                                            beer.capacity = Helper.ConvertPTNumberStrToDouble("0," + capacity);
                                        }
                                        else if (tcValues.ElementAt(1).Contains("l"))
                                        {
                                            capacity = tcValues.ElementAt(1).Replace("l", "");
                                            beer.capacity = Helper.ConvertPTNumberStrToDouble("0," + capacity);
                                        }
                                        else
                                        {
                                            capacity = tcValues.ElementAt(1).Replace("cl", "");
                                            beer.capacity = Helper.ConvertPTNumberStrToDouble("0," + capacity);
                                        }
                                    }
                                }
                            }
                        }

                        #endregion

                        #region rebuild name

                        var exclNames = new[] { "cl", "x" };

                        List<string> nameVals = beer.name.Split(' ').ToList();
                        beer.name = string.Empty;
                        foreach (string val in nameVals)
                        {
                            if (!exclNames.Any(val.Contains))
                                beer.name += val + " ";
                        }

                        beer.name.Trim();

                        #endregion

                        #region parse price

                        string priceVal = beerHtml.Attr("data-price");
                        if (!String.IsNullOrEmpty(priceVal))
                        {
                            double price = Helper.ConvertPTNumberStrToDouble(priceVal);

                            beer.priceBefore = Math.Round(price, 2, MidpointRounding.AwayFromZero);
                            beer.priceAfter = beer.priceBefore;
                        }

                        #endregion

                        #region parse discount

                        beer.discountNote = string.Empty;

                        string discount = beerHtml.Attr("data-size");
                        if (!String.IsNullOrEmpty(discount))
                        {
                            beer.hasDiscount = true;

                            Elements oldPriceHtml = beerHtml.GetElementsByClass("striked");
                            if (oldPriceHtml.Any())
                            {
                                string oldPriceVal = oldPriceHtml.First().Text().Replace("€", "").Trim();

                                double price = Helper.ConvertPTNumberStrToDouble(oldPriceVal);
                                beer.priceBefore = Math.Round(price, 2, MidpointRounding.AwayFromZero);
                            }
                        }

                        #endregion

                        #region calculate price per litre

                        double tCapacity = beer.total * beer.capacity;
                        double pricePerLitre = beer.priceAfter / tCapacity;

                        beer.pricePerLitre = Math.Round(pricePerLitre, 2, MidpointRounding.AwayFromZero);

                        #endregion

                        #region set beer urls

                        if (beer.id != null)
                        {
                            beer.imageUrl = String.Format(URL_PRODUCT_IMAGE, beer.id);
                            beer.detailsUrl = String.Format(URL_PRODUCT_DETAILS, beer.id);
                        }

                        #endregion

                        #region calculte price per unity

                        if (beer.priceAfter > 0 && beer.total > 0)
                        {
                            beer.priceUnity = Math.Round(beer.priceAfter / beer.total, 2, MidpointRounding.AwayFromZero);
                        }

                        #endregion

                        _DbFromFroiz.Add(beer);
                    }

                    if (_DbFromFroiz.Any())
                        await Helper.InsertBeersRecordAsync(_DbFromFroiz, STORE).ConfigureAwait(false);
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
                return _DbFromFroiz.OrderBy(x => x.pricePerLitre);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return _DbFromFroiz.OrderBy(x => x.pricePerLitre);
            }

            return _DbFromFroiz.OrderBy(x => x.pricePerLitre);
        }
    }
}
