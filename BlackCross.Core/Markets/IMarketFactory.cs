using System;

namespace BlackCross.Core.Markets
{
    public interface IMarketFactory
    {
        IMarketConfiguration GetConfiguration();

        MarketBase BuildMarket(IMarketConfiguration configuration);

        Type ConfigurationType { get; }
    }
}
