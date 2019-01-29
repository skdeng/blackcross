using System;

namespace BlackCross.Core
{
    public class MarketDataCandle
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public TimeSpan Duration => EndTime - StartTime;

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Open { get; set; }

        public decimal Close { get; set; }

        public decimal Volume { get; set; }
    }
}
