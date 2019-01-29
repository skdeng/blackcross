using BlackCross.Core.Markets;

namespace BlackCross.Core.TradeStrategies
{
    public abstract class SingleAssetStrategy : TradeStrategyBase
    {
        protected string Security { get; set; }

        protected string BaseCurrency { get; set; }

        protected SingleAssetStrategy(MarketBase market, string security, string baseCurrency) : base(market)
        {
            Security = security;
            BaseCurrency = baseCurrency;
        }

        public decimal PortfolioValue(decimal lastPrice)
        {
            return Market.Portfolio.TotalAmount(BaseCurrency) + Market.Portfolio.TotalAmount(Security) * lastPrice;
        }

    }
}
