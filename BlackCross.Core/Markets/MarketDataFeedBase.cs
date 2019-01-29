using System;
using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace BlackCross.Core.Markets
{
    /// <summary>
    /// Base class for market data feed
    /// </summary>
    public abstract class MarketDataFeedBase
    {
        public TimeSpan? TickInterval { get; set; }

        public virtual MarketDataFrame Current { get; protected set; }

        public virtual MarketDataFrame First { get; protected set; }

        public event EventHandler<MarketDataFrame> NewDataPoint;

        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MarketDataFeedBase(TimeSpan? tickInterval = null)
        {
            TickInterval = tickInterval;
        }

        public abstract MarketDataFrame Tick(TimeSpan? tickInterval = null);

        protected Task OnNewDataPoint(MarketDataFrame dataPoint)
        {
            return Task.Run(() => NewDataPoint?.Invoke(this, dataPoint));
        }
    }
}
