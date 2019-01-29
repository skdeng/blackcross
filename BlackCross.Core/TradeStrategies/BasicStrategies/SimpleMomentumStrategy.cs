using System;
using System.Collections.Generic;
using BlackCross.Core.Extensions;
using BlackCross.Core.Markets;

namespace BlackCross.Core.TradeStrategies.BasicStrategies
{
    public class SimpleMomentumStrategy : SingleAssetStrategy
    {
        private readonly int lookback;

        private readonly LinkedList<decimal> priceWindow;

        public SimpleMomentumStrategy(MarketBase market, string security, string baseCurrency, int lookback) : base(market, security, baseCurrency)
        {
            base.Market = market;
            this.lookback = lookback;

            priceWindow = new LinkedList<decimal>();
        }

        protected override void TradeInternal()
        {
            while (IsRunning)
            {
                try
                {
                    var datapoint = Market.Tick();
                    Market.CancelAllOrders();

                    var lastPrice = datapoint[Security].Price;

                    priceWindow.AddLast(lastPrice);
                    if (priceWindow.Count < lookback)
                    {
                        continue;
                    }
                    if (priceWindow.Count > lookback)
                    {
                        priceWindow.RemoveFirst();
                    }

                    var tradeVolume = 0m;
                    if (priceWindow.IsAscending() || priceWindow.IsDescending())
                    {
                        var priceDelta = lastPrice - priceWindow.First.Value;
                        var deltaRatio = priceDelta / lastPrice;

                        var securityRatio = 1 - Market.Portfolio.TotalAmount(BaseCurrency) / PortfolioValue(lastPrice);
                        var targetSecurityRatio = securityRatio + deltaRatio;

                        var targetSecurityAmount = Market.Portfolio.TotalAmount(Security) * targetSecurityRatio;
                        tradeVolume = targetSecurityAmount - Market.Portfolio.TotalAmount(Security);
                    }

                    var orderSide = OrderSide.Buy;
                    if (tradeVolume < 0)
                    {
                        orderSide = OrderSide.Sell;
                        tradeVolume = -tradeVolume;
                    }

                    if (tradeVolume > 1)
                    {
                        Market.OpenOrder(new Order()
                        {
                            BaseCurrency = BaseCurrency,
                            Security = Security,
                            Price = lastPrice,
                            Side = orderSide,
                            Type = OrderType.MarketOrder,
                            Volume = tradeVolume
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Strategy interrupted due to exception: {ex}");
                    break;
                }
            }
        }
    }
}
