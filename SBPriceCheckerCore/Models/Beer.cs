using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBPriceCheckerCore.Models
{
    public class Beer
    {
        public string id { get; set; }
        public string name { get; set; }
        public int total { get; set; }
        public double capacity { get; set; }
        public double priceBefore { get; set; }
        public double priceAfter { get; set; }
        public double pricePerLitre { get; set; }
        public double priceUnity { get; set; }
        public bool hasDiscount { get; set; }
        public string discountType { get; set; }
        public double discountValue { get; set; }
        public string discountNote { get; set; }
        public DateTime promoStart { get; set; }
        public DateTime promoEnd { get; set; }
        public string store { get; set; }
        public string imageUrl { get; set; }
        public string detailsUrl { get; set; }
    }
}
