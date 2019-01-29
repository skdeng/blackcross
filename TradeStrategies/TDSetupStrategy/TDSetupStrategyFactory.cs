using System;
using BlackCross.Core.Markets;
using BlackCross.Core.TradeStrategies;

namespace BlackCross.TradeStrategies.TDSetupStrategy
{
    public class TDSetupStrategyFactory : ITradeStrategyFactory
    {
        public Type ConfigurationType => typeof(TDSetupStrategyConfiguration);

        public TradeStrategyBase BuildStrategy(MarketBase market, IStrategyConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public IStrategyConfiguration GetConfiguration()
        {
            return new TDSetupStrategyConfiguration();
        }
    }
}
