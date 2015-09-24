using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBPriceCheckerCore.Models
{
    public class Beer
    {
        public int id { get; set; }
        public string name { get; set; }
        public int total { get; set; }
        public double capacity { get; set; }
        public double price { get; set; }
        public double pricePerLitre { get; set; }
        public bool hasDiscount { get; set; }
        public string discountType { get; set; }
        public double discountValue { get; set; }
    }
}
