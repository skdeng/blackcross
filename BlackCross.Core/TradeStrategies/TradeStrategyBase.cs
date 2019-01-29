using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BlackCross.Core.Markets;
using log4net;

namespace BlackCross.Core.TradeStrategies
{
    public abstract partial class TradeStrategyBase
    {
        public readonly string StrategyName;

        public List<Trade> ExecutedTrades { get; private set; }

        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected MarketBase Market { get; set; }

        protected bool IsRunning { get; set; }

        private Task _TradeTask;

        protected TradeStrategyBase(MarketBase market, string strategyName = null)
        {
            Market = market;
            ExecutedTrades = new List<Trade>();
            IsRunning = false;

            StrategyName = strategyName ?? GetType().Name;
        }

        public Task StartAsync()
        {
            _TradeTask = Task.Run((Action)Start);
            return _TradeTask;
        }

        public void Start()
        {
            if (Market == null)
            {
                Log.Error("Market is not set");
                throw new ApplicationException("Market is not set for strategy");
            }

            // Keep track of all executed trades
            Market.TradeExecuted += RecordExecutedTrades;

            IsRunning = true;
            TradeInternal();
        }

        public async Task Stop()
        {
            IsRunning = false;
            await _TradeTask;
        }

        protected abstract void TradeInternal();

        private void RecordExecutedTrades(object sender, Trade trade)
        {
            ExecutedTrades.Add(trade);
        }
    }
}
