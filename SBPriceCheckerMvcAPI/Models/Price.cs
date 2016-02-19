using System;

namespace SBPriceCheckerMvcAPI.Models
{
    public class Price
    {
        public int PriceID { get; set; }

        public int BeerID { get; set; }

        public double Value { get; set; }

        public DateTime Date { get; set; }

        public virtual Beer Beer { get; set; }
    }
}