using System;
using System.Collections.Generic;
using System.Linq;
using BlackCross.Core;
using BlackCross.Core.Markets;

namespace BlackCross.SandBox.HistoricalTest.Markets
{
    public class HistoricalMarket : MarketBase
    {
        private HashSet<Order> pendingOrders;

        private readonly decimal tradeFee;

        public HistoricalMarket(TimeSpan dataFeedInterval, Portfolio startingPortfolio, decimal tradeFee) : base("Historical Market")
        {
            DataFeed = new HistoricalMarketDataFeed(dataFeedInterval);
            pendingOrders = new HashSet<Order>();
            this.tradeFee = tradeFee;
            Portfolio = startingPortfolio;
        }

        public void LoadDataFromCsv(string filename, string security, string baseCurrency)
        {
            (DataFeed as HistoricalMarketDataFeed).LoadDataFromCsv(filename, security, baseCurrency);
        }

        public override IEnumerable<Order> CancelAllOrders()
        {
            foreach (var order in pendingOrders)
            {
                OnOrderCancelled(order);
            }
            pendingOrders.Clear();
            return Enumerable.Empty<Order>();
        }

        public override bool CancelOrder(Order order)
        {
            if (pendingOrders.Remove(order))
            {
                OnOrderCancelled(order);
                return true;
            }
            return false;
        }

        public override bool CancelOrder(string clientOrderId)
        {
            var orderToRemove = pendingOrders.Where(o => o.ClientOrderId == clientOrderId);
            if (orderToRemove.Count() == 1)
            {
                OnOrderCancelled(orderToRemove.First());
                return true;
            }
            else if (orderToRemove.Count() > 1)
            {
                throw new Exception($"Found more than 1 order with clientId: {clientOrderId}");
            }
            else
            {
                return false;
            }
        }

        public override decimal GetBenchmarkReturn(string security)
        {
            var securityStartingPrice = DataFeed.First[security].Price;
            var securityEndingPrice = DataFeed.Current[security].Price;

            return securityEndingPrice / securityStartingPrice;
        }

        public override MarketDataFrame GetLastPrice()
        {
            return DataFeed.Current;
        }

        public override Order OpenOrder(Order order)
        {
            OnOrderOpened(order);
            if (order.Type == OrderType.MarketOrder)
            {
                return ProcessMarketOrder(order, DataFeed.Current[order.Security]);
            }
            else
            {
                if (!ProcessLimitOrder(order, DataFeed.Current[order.Security]))
                {
                    switch (order.Side)
                    {
                        case OrderSide.Buy:
                            Portfolio.FreezeSecurity(order.BaseCurrency, order.Volume * order.Price * (1 + tradeFee));
                            break;
                        case OrderSide.Sell:
                            Portfolio.FreezeSecurity(order.Security, order.Volume);
                            break;
                    }

                    pendingOrders.Add(order);
                }
                return order;
            }
        }

        public override MarketDataFrame Tick()
        {
            var dataPoint = DataFeed.Tick();

            foreach (var order in pendingOrders)
            {
                switch (order.Type)
                {
                    case OrderType.LimitOrder:
                        ProcessLimitOrder(order, dataPoint[order.Security]);
                        break;
                    default:
                        throw new Exception($"Unsupported order type: {order.Type}");
                }
            }

            return dataPoint;
        }

        private Order ProcessMarketOrder(Order order, MarketDataPoint currentMarketDataPoint)
        {
            var orderProcessed = false;
            switch (order.Side)
            {
                case OrderSide.Buy:
                    if (Portfolio.RemoveSecurity(order.BaseCurrency, order.Volume * currentMarketDataPoint.Price * (1 + tradeFee)))
                    {
                        Portfolio.AddSecurity(order.Security, order.Volume);
                        orderProcessed = true;
                    }
                    break;
                case OrderSide.Sell:
                    if (Portfolio.RemoveSecurity(order.Security, order.Volume))
                    {
                        Portfolio.AddSecurity(order.BaseCurrency, order.Volume * currentMarketDataPoint.Price * (1 - tradeFee));
                        orderProcessed = true;
                    }
                    break;
            }
            if (orderProcessed)
            {
                OnTradeExecuted(new Trade()
                {
                    Timestamp = currentMarketDataPoint.Timestamp,
                    Order = order,
                    ExecutionPrice = currentMarketDataPoint.Price,
                    ExecutionVolume = order.Volume
                });
                return order;
            }
            else
            {
                logger.Warn($"Unable to place order: {order.ToPrettyString()}");
                return null;
            }
        }

        private bool ProcessLimitOrder(Order order, MarketDataPoint currentMarketDataPoint)
        {
            var orderProcessed = false;
            switch (order.Side)
            {
                case OrderSide.Buy:
                    if (order.Price >= currentMarketDataPoint.Price)
                    {
                        if (Portfolio.UnfreezeSecurity(order.BaseCurrency, order.Volume * order.Price * (1 + tradeFee)) &&
                            Portfolio.RemoveSecurity(order.BaseCurrency, order.Volume * currentMarketDataPoint.Price * (1 + tradeFee)))
                        {
                            Portfolio.AddSecurity(order.Security, order.Volume);
                            orderProcessed = true;
                        }
                    }
                    break;
                case OrderSide.Sell:
                    if (order.Price <= currentMarketDataPoint.Price)
                    {
                        if (Portfolio.UnfreezeSecurity(order.Security, order.Volume) && Portfolio.RemoveSecurity(order.Security, order.Volume))
                        {
                            var transactionAmount = order.Volume * currentMarketDataPoint.Price * (1 - tradeFee);
                            Portfolio.AddSecurity(order.BaseCurrency, transactionAmount);
                            orderProcessed = true;
                        }
                    }
                    break;
            }

            if (orderProcessed)
            {
                pendingOrders.Remove(order);
                OnTradeExecuted(new Trade()
                {
                    Timestamp = currentMarketDataPoint.Timestamp,
                    Order = order,
                    ExecutionPrice = currentMarketDataPoint.Price,
                    ExecutionVolume = order.Volume
                });
            }
            else
            {
                logger.Warn($"Cannot place order: {order.ToPrettyString()}");
            }

            return orderProcessed;
        }
    }
}
