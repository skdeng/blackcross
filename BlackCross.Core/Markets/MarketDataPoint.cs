using System;

namespace BlackCross.Core
{
    public class MarketDataPoint
    {
        public DateTime Timestamp { get; set; }

        public decimal Price { get; set; }

        public string Security { get; set; }

        public string BaseCurrency { get; set; }
    }
}
