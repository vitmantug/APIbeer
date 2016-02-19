using SBPriceCheckerMvcAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBPriceCheckerMvcAPI.DAL
{
    public class APIBeerInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<APIBeerContext>
    {
        protected override void Seed(APIBeerContext context)
        {
            var beers = new List<Beer>
            {
                new Beer
                {
                    id = "48591",
                    name = "Cerveja Super Bock lata ",
                    total = 1,
                    capacity = 0.5,
                    priceBefore = 0.99,
                    priceAfter = 0.69,
                    pricePerLitre = 1.38,
                    priceUnity = 0.69,
                    hasDiscount = true,
                    discountType = null,
                    discountValue = 0.0,
                    discountNote = "",
                    promoStart = DateTime.Parse("0001-01-01T00:00:00"),
                    promoEnd = DateTime.Parse("0001-01-01T00:00:00"),
                    store = "Froiz",
                    imageUrl = "https://loja.froiz.com/fotopro/thumbnails/48591.jpg",
                    detailsUrl = "https://loja.froiz.com/product/48591/"
                }
            };

            beers.ForEach(s => context.Beers.Add(s));
            context.SaveChanges();

            var prices = new List<Price>
            {
                new Price { BeerID = 1, Value = 0.69, Date = DateTime.Now }
            };

            prices.ForEach(s => context.Prices.Add(s));
            context.SaveChanges();
        }
    }
}
