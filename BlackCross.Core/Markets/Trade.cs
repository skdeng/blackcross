using System;

namespace BlackCross.Core
{
    public class Trade
    {
        public DateTime Timestamp { get; set; }

        public Order Order { get; set; }

        public decimal ExecutionPrice { get; set; }

        public decimal ExecutionVolume { get; set; }
    }
}
