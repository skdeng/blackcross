using System;
using System.Collections.Generic;

namespace BlackCross.Core.Markets
{
    public class MarketDataFrame
    {
        public DateTime Timestamp { get; set; }

        public MarketDataPoint this[string symbol] => _PriceData.ContainsKey(symbol) ? _PriceData[symbol] : null;

        private Dictionary<string, MarketDataPoint> _PriceData { get; set; }

        public MarketDataFrame(DateTime timestamp, Dictionary<string, MarketDataPoint> data = null)
        {
            Timestamp = timestamp;
            _PriceData = data ?? new Dictionary<string, MarketDataPoint>();
        }

        public void AddPriceData(string symbol, MarketDataPoint dataPoint)
        {
            _PriceData.Add(symbol, dataPoint);
        }
    }
}
