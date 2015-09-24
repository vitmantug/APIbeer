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

namespace SBPriceCheckerCore.Parsers
{
    public class Continente
    {
        private static string URL_SEARCH = "http://www.continente.pt/pt-pt/public/Pages/searchresults.aspx?k=super%20bock#/?pl=160";

        private Helper Helper = new Helper();

        public IEnumerable<Beer> GetBeers()
        {
            List<Beer> _DbFromContinente = new List<Beer>();

            Document webpageHtml = NSoupClient.Parse(new WebClient().OpenRead(URL_SEARCH), "UTF-8");

            Elements beersHtml = webpageHtml.Body.SiblingElements.Select("div.productBox");

            foreach (Element beerHtml in beersHtml)
            {
                Beer beer = new Beer();

                #region parse Id

                Elements idHtml = beerHtml.SiblingElements.Select("input.item_pid");
                if (idHtml.Any())
                {
                    string idValue = idHtml.First().Attr("value");
                    if (!String.IsNullOrEmpty(idValue))
                    {
                        List<string> values = idValue.Split('(').ToList<string>();
                        if(values.Any())
                        {
                            string value = values.ElementAt(0);
                            int id = 0;
                            if (int.TryParse(value, out id))
                            {
                                beer.Id = id;
                            }
                        }
                    }
                }

                #endregion

                #region parse name

                string name = beerHtml.SiblingElements.Select("div.productTitle").Text;
                beer.Name = name;

                #endregion

                #region parse total and capacity

                string totalCapacity = beerHtml.SiblingElements.Select("div.productSubtitle").Text;
                if (!String.IsNullOrEmpty(totalCapacity))
                {
                    string total = string.Empty;
                    string capacity = string.Empty;
                    string unity = string.Empty;

                    List<string> totalCapacityElems = totalCapacity.Split(' ').ToList<string>();
                    int n = totalCapacityElems.Count;

                    switch (n)
                    {
                        case 3:

                            beer.Total = 1;

                            capacity = totalCapacityElems.ElementAt(1);
                            unity = totalCapacityElems.ElementAt(2);

                            if (unity.Equals("cl"))
                            {
                                beer.Capacity = Helper.ConvertPTNumberStrToDouble("0," + capacity);
                            }
                            else if (unity.Equals("lt"))
                            {
                                beer.Capacity = Helper.ConvertPTNumberStrToDouble(capacity);
                            }

                            break;

                        case 5:

                            total = totalCapacityElems.ElementAt(1);
                            beer.Total = Convert.ToInt32(total);

                            capacity = totalCapacityElems.ElementAt(3);
                            unity = totalCapacityElems.ElementAt(4);

                            if (unity.Equals("cl"))
                            {
                                beer.Capacity = Helper.ConvertPTNumberStrToDouble("0," + capacity);
                            }
                            else if (unity.Equals("lt"))
                            {
                                beer.Capacity = Helper.ConvertPTNumberStrToDouble(capacity);
                            }

                            break;
                        default:
                            break;
                    }
                }

                #endregion

                #region parse price

                string priceHtml = beerHtml.SiblingElements.Select("div.pricePerUnit").Text;
                if (!String.IsNullOrEmpty(priceHtml))
                {
                    List<string> priceElems = priceHtml.Split(' ').ToList<string>();
                    if (priceElems.Any() && priceElems.Count > 1)
                    {
                        beer.Price = Helper.ConvertPTNumberStrToDouble(priceElems.ElementAt(1));
                    }
                }

                #endregion

                #region parse discount

                Elements discountHtml = beerHtml.SiblingElements.Select("input.HasWebDiscounts");
                if (discountHtml.Any())
                {
                    string hasDiscount = discountHtml.First().Attr("value");
                    beer.hasDiscount = hasDiscount.Equals("false") ? false : true;
                }

                if (beer.hasDiscount == true)
                {
                    Elements discountTypeHtml = beerHtml.SiblingElements.Select("input.WebDiscountType");
                    if (discountTypeHtml.Any())
                    {
                        beer.DiscountType = discountTypeHtml.First().Attr("value");
                    }

                    Elements discountValueHtml = beerHtml.SiblingElements.Select("input.WebDiscountValue");
                    if (discountValueHtml.Any())
                    {
                        string discountValue = discountValueHtml.First().Attr("value");
                        beer.DiscountValue = Helper.ConvertPTNumberStrToDouble(discountValue);
                    }
                }

                #endregion

                #region parse price per litre

                string priceLHtml = beerHtml.SiblingElements.Select("div.pricePerLitre").Text;
                if (!String.IsNullOrEmpty(priceLHtml))
                {
                    List<string> priceLElems = priceLHtml.Split(' ').ToList<string>();
                    if (priceLElems.Any() && priceLElems.Count > 1)
                    {
                        beer.PricePerLitre = Helper.ConvertPTNumberStrToDouble(priceLElems.ElementAt(1));
                    }
                }

                if (beer.hasDiscount == true && !String.IsNullOrEmpty(beer.DiscountType))
                {
                    if (beer.DiscountType.Equals("Value"))
                    {
                        double newPrice = beer.Price - beer.DiscountValue;
                        double tCapacity = beer.Total * beer.Capacity;
                        double newPriceL = newPrice / tCapacity;

                        beer.PricePerLitre = Math.Round(newPriceL, 2, MidpointRounding.AwayFromZero);
                    }
                    else if (beer.DiscountType.Equals("Percentage"))
                    {
                        double newPrice = beer.Price * beer.DiscountValue / 100;
                        double tCapacity = beer.Total * beer.Capacity;
                        double newPriceL = newPrice / tCapacity;

                        beer.PricePerLitre = Math.Round(newPriceL, 2, MidpointRounding.AwayFromZero);
                    }
                }

                #endregion

                _DbFromContinente.Add(beer);
            }

            return _DbFromContinente.OrderBy(x => x.PricePerLitre);
        }
    }
}
