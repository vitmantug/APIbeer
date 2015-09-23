using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBPriceCheckerCore.Models
{
    public class Beer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Total { get; set; }
        public double Capacity { get; set; }
        public double Price { get; set; }
        public double PricePerLitre { get; set; }
        public bool hasDiscount { get; set; }
        public string DiscountType { get; set; }
        public double DiscountValue { get; set; }
    }
}
