﻿using System;
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

        public IQueryable<Beer> GetBeers()
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
                                beer.id = id;
                            }
                        }
                    }
                }

                #endregion

                #region parse name

                string name = beerHtml.SiblingElements.Select("div.productTitle").Text;
                beer.name = name;

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

                            beer.total = 1;

                            capacity = totalCapacityElems.ElementAt(1);
                            unity = totalCapacityElems.ElementAt(2);

                            if (unity.Equals("cl"))
                            {
                                beer.capacity = Helper.ConvertPTNumberStrToDouble("0," + capacity);
                            }
                            else if (unity.Equals("lt"))
                            {
                                beer.capacity = Helper.ConvertPTNumberStrToDouble(capacity);
                            }

                            break;

                        case 5:

                            total = totalCapacityElems.ElementAt(1);
                            beer.total = Convert.ToInt32(total);

                            capacity = totalCapacityElems.ElementAt(3);
                            unity = totalCapacityElems.ElementAt(4);

                            if (unity.Equals("cl"))
                            {
                                beer.capacity = Helper.ConvertPTNumberStrToDouble("0," + capacity);
                            }
                            else if (unity.Equals("lt"))
                            {
                                beer.capacity = Helper.ConvertPTNumberStrToDouble(capacity);
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
                        beer.price = Helper.ConvertPTNumberStrToDouble(priceElems.ElementAt(1));
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
                        beer.discountType = discountTypeHtml.First().Attr("value");
                    }

                    Elements discountValueHtml = beerHtml.SiblingElements.Select("input.WebDiscountValue");
                    if (discountValueHtml.Any())
                    {
                        string discountValue = discountValueHtml.First().Attr("value");
                        beer.discountValue = Helper.ConvertPTNumberStrToDouble(discountValue);
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
                        beer.pricePerLitre = Helper.ConvertPTNumberStrToDouble(priceLElems.ElementAt(1));
                    }
                }

                if (beer.hasDiscount == true && !String.IsNullOrEmpty(beer.discountType))
                {
                    if (beer.discountType.Equals("Value"))
                    {
                        double newPrice = beer.price - beer.discountValue;
                        double tCapacity = beer.total * beer.capacity;
                        double newPriceL = newPrice / tCapacity;

                        beer.pricePerLitre = Math.Round(newPriceL, 2, MidpointRounding.AwayFromZero);
                    }
                    else if (beer.discountType.Equals("Percentage"))
                    {
                        double newPrice = beer.price * beer.discountValue / 100;
                        double tCapacity = beer.total * beer.capacity;
                        double newPriceL = newPrice / tCapacity;

                        beer.pricePerLitre = Math.Round(newPriceL, 2, MidpointRounding.AwayFromZero);
                    }
                }

                #endregion

                _DbFromContinente.Add(beer);
            }

            //return _DbFromContinente.OrderBy(x => x.PricePerLitre);
            return _DbFromContinente.AsQueryable();
        }
    }
}
