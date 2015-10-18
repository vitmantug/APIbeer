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

namespace SBPriceCheckerCore.Parsers
{
    public class ElCorteIngles
    {
        private static string URL_PRODUCT_DETAILS = "http://www.elcorteingles.pt/supermercado/sm2/pt_PT/520142/supermarket/bebidas/aguas-sumos-e-cervejas/cervejas/cerveja/{0}";
        
        private static string URL_LOGIN = "http://www.elcorteingles.pt/supermercado/sm2/login/assignStore.jsp?shopsupermarket&centreId=520142&isPickup=true&shipMth=selfPickup&action=editPymeEditUser&extendData=&fromRegistry=true&externalIntranet=&virtualCenter=&deliveryCentre=&uid=1w4lxL05r9aFCUa31UCO1AVhXOZIhTLG7sMztNPxw-4=";
        private static string URL_SEARCH = "http://www.elcorteingles.pt/supermercado/sm2/search/resultsProductList.jsp?total_results=19&N=0&Ntt=super+bock&Nty=1&Ntx=mode+matchall&Ntk=SearchDescription&Nr=AND(product.catalogId:010_1,availableCentres:520142)&Np=2&Ntpc=1&Ntpr=1&Nao=0&Nrr=first,field,nterms,exact,numfields,static(varietyDescriptionProperty,ascending)&Nrt=super+bock&Nrk=All&Nrm=matchAll&_requestid=492511";

        private Helper Helper = new Helper();

        public async Task<IEnumerable<Beer>> GetBeers()
        {
            List<Beer> _DbFromElCorteIngles = new List<Beer>();

            try
            {
                HttpWebRequest webReq = (HttpWebRequest)HttpWebRequest.Create(URL_LOGIN);
                #region HttpWebRequest headers
                webReq.CookieContainer = new CookieContainer();
                webReq.Method = "GET";
                webReq.Host = "www.elcorteingles.pt";
                webReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                webReq.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.71 Safari/537.36";
                webReq.Headers.Add("Accept-Encoding", "gzip, deflate, sdch");
                webReq.Headers.Add("Accept-Language", "pt-PT,pt;q=0.8,en-US;q=0.6,en;q=0.4,es;q=0.2");
                webReq.Headers.Add("Upgrade-Insecure-Requests", "1");

                //webReq.CookieContainer.Add(new Cookie("ASPSESSIONIDACSDDQQD", "IGEOJPPADBPAKKNBIEFPAOON") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("ASPSESSIONIDACTSDTQT", "NBIHGCKDOOGMEBKNFFBKOHBE") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("FECIPTTAM6", "!9i0fscZhCWk5ZRxQqvfntZFj47Q97l0q5ZQhTz8wDlr5VCpmIQ+0l9C9bZoBFi1X1N2AQ0awvQ==") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("JSESSIONID_PROFILE", "00009Jl1j5c6ecoNcGBDvj4tnzP:17mjmcfjh") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("JSESSIONID_SUPERMARKET", "0000inXf-C1XCQHIo3fUKyjzUGW:17mjm7enm") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("PD-H-SESSION-ID", "4_0_CAJZFlTHOdq-LQhUgk0-LJmhjBrgC7SpJ1HAflESvhN5cqrK") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("SITESERVER", "ID=db76594c242bc6f2113ae3de68e1752a") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("__atuvc", "3%7C41") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("_ga", "GA1.2.1595062123.1444947849") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("_gat_UA-42384899-16", "1") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("_hjIncludedInSample", "0") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("cmRS", "t3=1444948206166&pi=CAMPANHAS%3ADESTAQUES") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("cmTPSet", "Y") { Domain = webReq.Host });
                //webReq.CookieContainer.Add(new Cookie("cookiesPolicy", "1") { Domain = webReq.Host });
                #endregion
                WebResponse response = await webReq.GetResponseAsync();

                HttpWebRequest webReq2 = (HttpWebRequest)HttpWebRequest.Create(URL_SEARCH);
                #region HttpWebRequest headers
                webReq2.CookieContainer = webReq.CookieContainer;
                webReq2.Method = "GET";
                webReq2.Host = "www.elcorteingles.pt";
                webReq2.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                webReq2.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.71 Safari/537.36";
                webReq2.Headers.Add("Accept-Encoding", "gzip, deflate, sdch");
                webReq2.Headers.Add("Accept-Language", "pt-PT,pt;q=0.8,en-US;q=0.6,en;q=0.4,es;q=0.2");
                webReq2.Headers.Add("Upgrade-Insecure-Requests", "1");
                #endregion
                WebResponse response2 = await webReq2.GetResponseAsync();

                Document webpageHtml = NSoupClient.Parse(response2.GetResponseStream(), "utf-8");
                Elements beersHtml = webpageHtml.Body.SiblingElements.Select("div.product-tile");

                foreach (Element beerHtml in beersHtml)
                {
                    Beer beer = new Beer();
                    beer.store = "ElCorteIngles";

                    #region parse data-json

                    //{\"id\":\"0105218600300315___\",\n\"name\":\"Super Bock Green Cerveja 33 cl\",\n\"brand\":\"Super Bock Green\",\n\"final_price\":\"0.97\",\n\"status\":\"Disponible\",\n\"currency\":\"EUR\"}
                    string dataJson = beerHtml.Attr("data-json");

                    dynamic objectJson = JsonConvert.DeserializeObject<dynamic>(dataJson);

                    #endregion

                    #region parse id

                    beer.id = objectJson["id"];

                    #endregion

                    #region parse name

                    beer.name = objectJson["name"];

                    #endregion

                    #region parse price

                    string priceValue = objectJson["final_price"];

                    if (!String.IsNullOrEmpty(priceValue))
                    {
                        double price = Helper.ConvertPTNumberStrToDouble(priceValue);

                        beer.priceBefore = Math.Round(price, 2, MidpointRounding.AwayFromZero);
                        beer.priceAfter = beer.priceBefore;
                    }

                    #endregion

                    #region set beer urls

                    beer.imageUrl = beerHtml.GetElementsByClass("js-tile-image").Attr("data-image");

                    if (!String.IsNullOrEmpty(beer.id))
                    {
                        beer.detailsUrl = String.Format(URL_PRODUCT_DETAILS, beer.id);
                    }

                    #endregion

                    #region parse discount

                    string promoTxt = objectJson["vendor_product_promotion"];

                    if (!String.IsNullOrEmpty(promoTxt))
                    {
                        beer.discountNote = promoTxt;

                        List<string> values = promoTxt.Split(' ').ToList<string>();
                        foreach (string value in values)
                        {
                            if (value.Contains("%"))
                            {
                                beer.hasDiscount = true;
                                beer.discountType = "Percentage";

                                string val = value.Replace("%", "");

                                beer.discountValue = Helper.ConvertPTNumberStrToDouble(val);
                            }
                        }
                    }

                    #endregion

                    #region parse total and capacity

                    if (!String.IsNullOrEmpty(beer.name))
                    {
                        string total = string.Empty;
                        string capacity = string.Empty;
                        string unity = string.Empty;

                        List<string> toRemove = new List<string>() { "garrafa", "embalagem", "Pack" };

                        List<string> totalCapacityElems = beer.name.Split(' ').ToList<string>(); //Super Bock Stout Cerveja Nacional Preta Pack 6 garrafa 33 cl
                        int n = totalCapacityElems.Count;

                        unity = totalCapacityElems.ElementAt(n - 1);
                        capacity = totalCapacityElems.ElementAt(n - 2);

                        if (unity.Equals("cl"))
                        {
                            beer.capacity = Helper.ConvertPTNumberStrToDouble("0," + capacity);
                        }
                        else if (unity.Equals("L"))
                        {
                            beer.total = 1;
                            beer.capacity = Helper.ConvertPTNumberStrToDouble(capacity);
                        }

                        //Remove unity and capacity
                        totalCapacityElems.RemoveRange(n - 2, 2);
                        n = totalCapacityElems.Count;

                        //Parse total
                        total = totalCapacityElems.ElementAt(n - 1);

                        int totalValue = 0;
                        if (int.TryParse(total, out totalValue))
                        {
                            beer.total = totalValue;

                            totalCapacityElems.RemoveAt(n - 1);
                            n = totalCapacityElems.Count;
                        }
                        else
                        {
                            if (total.Equals("Cerveja"))
                            {
                                beer.total = 1;

                                totalCapacityElems.RemoveAt(n - 1);
                                n = totalCapacityElems.Count;
                            }
                            else if(total.Equals("Branca"))
                            {
                                beer.total = 1;
                            }
                        }

                        //Reassemble name
                        beer.name = "";
                        foreach(string name in totalCapacityElems)
                        {
                            beer.name += name + " ";
                        }
                        beer.name.Trim();

                        List<string> totalElems = beer.name.Split(new string[] { "Leve" }, StringSplitOptions.None).ToList<string>();
                        if (totalElems.Any() && totalElems.Count == 2)
                        {
                            beer.name = totalElems.ElementAt(0).Trim();

                            string tVal = totalElems.ElementAt(1).Trim().Split(' ').ToList<string>().ElementAt(0);
                            beer.total = Convert.ToInt32(tVal);
                        }
                        else if (totalElems.Any() && totalElems.Count == 1)
                        {
                            List<string> totalElemsPack = beer.name.Split(new string[] { "Pack " }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                            if (totalElemsPack.Any() && totalElemsPack.Count == 2)
                            {
                                beer.name = totalElemsPack.ElementAt(0).Trim();

                                string tVal = totalElemsPack.ElementAt(1).Trim().Split(' ').ToList<string>().ElementAt(0);
                                beer.total = Convert.ToInt32(tVal);
                            }
                            else if (totalElemsPack.Any() && totalElemsPack.Count == 1)
                            {
                                beer.name = totalElemsPack.ElementAt(0).Trim();
                            }
                        }

                        foreach (string strToRem in toRemove)
                        {
                            beer.name = beer.name.Replace(strToRem, "").Trim();
                        }
                    }

                    #endregion

                    #region parse price per litre

                    if (beer.hasDiscount == true && !String.IsNullOrEmpty(beer.discountType))
                    {
                        if (beer.discountType.Equals("Percentage"))
                        {
                            double newPrice = beer.priceBefore * beer.discountValue / 100;
                            beer.priceAfter = Math.Round(newPrice, 2, MidpointRounding.AwayFromZero);

                            double tCapacity = beer.total * beer.capacity;
                            double newPriceL = newPrice / tCapacity;
                            beer.pricePerLitre = Math.Round(newPriceL, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    else
                    {
                        double tCapacity = beer.total * beer.capacity;
                        double priceL = beer.priceAfter / tCapacity;
                        beer.pricePerLitre = Math.Round(priceL, 2, MidpointRounding.AwayFromZero);
                    }

                    #endregion

                    _DbFromElCorteIngles.Add(beer);
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
                return _DbFromElCorteIngles.OrderBy(x => x.pricePerLitre);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return _DbFromElCorteIngles.OrderBy(x => x.pricePerLitre);
            }

            return _DbFromElCorteIngles.OrderBy(x => x.pricePerLitre);
        }
    }
}
