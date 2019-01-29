using System;
using BlackCross.Core.Markets;

namespace BlackCross.Core.TradeStrategies
{
    public interface ITradeStrategyFactory
    {
        IStrategyConfiguration GetConfiguration();

        TradeStrategyBase BuildStrategy(MarketBase market, IStrategyConfiguration configuration);

        Type ConfigurationType { get; }
    }
}
