using System;
using BlackCross.Core.Markets;
using BlackCross.Core.TradeStrategies;

namespace BlackCross.TradeStrategies.SimpleMeanReversalStrategy
{
    public class SimpleMeanReversalStrategyFactory : ITradeStrategyFactory
    {
        public Type ConfigurationType => typeof(SimpleMeanReversalStrategyConfiguration);

        public TradeStrategyBase BuildStrategy(MarketBase market, IStrategyConfiguration configuration)
        {
            if (!(configuration is SimpleMeanReversalStrategyConfiguration config))
            {
                throw new ArgumentException();
            }
            else
            {
                return new SimpleMeanReversalStrategy(market, config.SecuritySymbol, config.BaseCurrencySymbol, config.Lookback, config.TradeFrequency);
            }
        }

        public IStrategyConfiguration GetConfiguration()
        {
            return new SimpleMeanReversalStrategyConfiguration();
        }
    }
}
