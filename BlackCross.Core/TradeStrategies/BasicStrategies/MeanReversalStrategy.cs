using System.Collections.Generic;
using System.Linq;
using BlackCross.Core.Extensions;
using BlackCross.Core.Markets;

namespace BlackCross.Core.TradeStrategies.BasicStrategies
{
    public class MeanReversalStrategy : SingleAssetStrategy
    {
        protected int LookBack { get; set; }

        protected decimal MovingAverage { get; set; }

        protected decimal MovingStdDeviation { get; set; }

        protected LinkedList<decimal> PriceWindow { get; set; }

        public MeanReversalStrategy(MarketBase market, string security, string baseCurrency, int lookback) : base(market, security, baseCurrency)
        {
            LookBack = lookback;
            PriceWindow = new LinkedList<decimal>();
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

                    PriceWindow.AddLast(lastPrice);
                    if (PriceWindow.Count < LookBack)
                    {
                        continue;
                    }

                    if (PriceWindow.Count > LookBack)
                    {
                        PriceWindow.RemoveFirst();
                    }

                    MovingAverage = PriceWindow.Average();
                    MovingStdDeviation = PriceWindow.StdDeviation();

                    var zvalue = -(lastPrice - MovingAverage) / MovingStdDeviation;
                    var tradeVolume = ComputeTradeVolume(zvalue, lastPrice);

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug($"Computed Moving Average: {MovingAverage}");
                        Log.Debug($"Computed Moving Std Deviation: {MovingStdDeviation}");
                        Log.Debug($"Computed Z-Value: {zvalue}");
                        Log.Debug($"Computed Trade Volume: {tradeVolume}");
                    }

                    var orderSide = tradeVolume > 0 ? OrderSide.Buy : OrderSide.Sell;
                    tradeVolume = tradeVolume < 0 ? -tradeVolume : tradeVolume;

                    if (tradeVolume > 1m)
                    {
                        var tradePrice = lastPrice;
                        var openOrder = Market.OpenOrder(BuildOrder(lastPrice, tradeVolume, OrderType.MarketOrder, orderSide));
                        if (openOrder != null)
                        {
                            Log.Info($"Placing order {openOrder.ToFormattedString()}");
                        }
                    }
                    else
                    {
                        Log.Info($"Trade volume {tradeVolume} too small, will not place order");
                    }
                }
                catch (DataFeedInterruptedException ex)
                {
                    Log.Error($"Data feed interrupted: {ex.DataFeedName}");
                    break;
                }
            }
        }

        protected virtual decimal ComputeTradeVolume(decimal zvalue, decimal lastPrice)
        {
            var currentCurrencyBalance = Market.Portfolio.TotalAmount(BaseCurrency);
            var currentSecurityBalance = Market.Portfolio.TotalAmount(Security);

            if (zvalue > 0)
            {
                var targetCurrencyBalance = PortfolioValue(lastPrice) / (2 + zvalue);
                return (currentCurrencyBalance - targetCurrencyBalance) / lastPrice;
            }
            else
            {
                var targetSecurityBalance = PortfolioValue(lastPrice) / (2 - zvalue);
                return targetSecurityBalance / lastPrice - currentSecurityBalance;
            }
        }

        private Order BuildOrder(decimal price, decimal volume, OrderType type, OrderSide side)
        {
            return new Order()
            {
                BaseCurrency = BaseCurrency,
                Security = Security,
                Type = type,
                Side = side,
                Price = price,
                Volume = volume
            };
        }
    }
}
