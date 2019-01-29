using System;
using System.Collections.Generic;
using System.Linq;
using BlackCross.Core;
using BlackCross.Core.Markets;
using BlackCross.Core.TradeStrategies;

namespace BlackCross.TradeStrategies.TDSetupStrategy
{
    public class TDSetupStrategy : SingleAssetStrategy
    {
        private readonly List<decimal> priceWindow;

        private const int windowSize = 13;

        public TDSetupStrategy(MarketBase market, string security, string baseCurrency) : base(market, security, baseCurrency)
        {
            priceWindow = new List<decimal>();
        }

        protected override void TradeInternal()
        {
            while (IsRunning)
            {
                try
                {
                    var dataPoint = Market.Tick();
                    Market.CancelAllOrders();

                    var lastPrice = dataPoint[Security].Price;
                    priceWindow.Add(lastPrice);

                    if (priceWindow.Count < windowSize)
                    {
                        continue;
                    }

                    var setupResult = TDSequentialIndicator.ComputeTDSetup(priceWindow);
                    var lastSetup = setupResult.Last();
                    OrderSide side;
                    if (lastSetup == 9)
                    {
                        side = OrderSide.Sell;
                    }
                    else if (lastSetup == -9)
                    {
                        side = OrderSide.Buy;
                    }
                    else
                    {
                        continue;
                    }

                    var tradeVolume = ComputeTradeVolume(side, lastPrice);

                    if (tradeVolume > 1)
                    {
                        Market.OpenOrder(new Order()
                        {
                            BaseCurrency = BaseCurrency,
                            Security = Security,
                            Price = lastPrice,
                            Side = side,
                            Type = OrderType.MarketOrder,
                            Volume = tradeVolume
                        });
                    }
                }
                catch (DataFeedInterruptedException ex)
                {
                    Log.Error($"Trade strategy interrupted due to exception: {ex}");
                    break;
                }
            }
        }

        private decimal ComputeTradeVolume(OrderSide side, decimal lastPrice)
        {
            switch (side)
            {
                case OrderSide.Buy:
                    return Market.Portfolio.TotalAmount(BaseCurrency) / (2 * lastPrice);
                case OrderSide.Sell:
                    return Market.Portfolio.TotalAmount(Security) / 2;
                default:
                    throw new Exception($"Unknown value: {side}");
            }
        }
    }
}
