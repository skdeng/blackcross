using System;
using System.Collections.Generic;
using System.Linq;
using BlackCross.Core;
using BlackCross.Core.Markets;

namespace BlackCross.Markets.HistoricalMarket
{
    public class HistoricalMarket : MarketBase
    {
        private HashSet<Order> _PendingOrders;

        private readonly decimal _TradeFee;

        public HistoricalMarket(TimeSpan dataFeedInterval, Portfolio startingPortfolio, decimal tradeFee) : base("Historical Market")
        {
            DataFeed = new HistoricalMarketDataFeed(dataFeedInterval);
            _PendingOrders = new HashSet<Order>();
            _TradeFee = tradeFee;
            Portfolio = startingPortfolio;
        }

        public void LoadDataFromCsv(string filename, string security, string baseCurrency)
        {
            (DataFeed as HistoricalMarketDataFeed).LoadDataFromCsv(filename, security, baseCurrency);
        }

        public override IEnumerable<Order> CancelAllOrders()
        {
            foreach (var order in _PendingOrders)
            {
                OnOrderCancelled(order);
            }
            _PendingOrders.Clear();
            return Enumerable.Empty<Order>();
        }

        public override bool CancelOrder(Order order)
        {
            if (_PendingOrders.Remove(order))
            {
                OnOrderCancelled(order);
                return true;
            }
            return false;
        }

        public override bool CancelOrder(string clientOrderId)
        {
            var orderToRemove = _PendingOrders.Where(o => o.ClientOrderId == clientOrderId);
            if (orderToRemove.Count() == 1)
            {
                Log.Info($"Cancelled order {orderToRemove.First().ToFormattedString()}");
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
            Log.Debug($"Opening order {order.ToFormattedString()}");
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
                            Portfolio.FreezeSecurity(order.BaseCurrency, order.Volume * order.Price * (1 + _TradeFee));
                            break;
                        case OrderSide.Sell:
                            Portfolio.FreezeSecurity(order.Security, order.Volume);
                            break;
                    }

                    _PendingOrders.Add(order);
                }
                return order;
            }
        }

        public override MarketDataFrame Tick(TimeSpan? tickInterval = null)
        {
            var dataPoint = DataFeed.Tick(tickInterval);

            foreach (var order in _PendingOrders)
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
            Log.Debug($"Processed market order {order}");
            var orderProcessed = false;
            switch (order.Side)
            {
                case OrderSide.Buy:
                    if (Portfolio.RemoveSecurity(order.BaseCurrency, order.Volume * currentMarketDataPoint.Price * (1 + _TradeFee)))
                    {
                        Portfolio.AddSecurity(order.Security, order.Volume);
                        orderProcessed = true;
                    }
                    break;
                case OrderSide.Sell:
                    if (Portfolio.RemoveSecurity(order.Security, order.Volume))
                    {
                        Portfolio.AddSecurity(order.BaseCurrency, order.Volume * currentMarketDataPoint.Price * (1 - _TradeFee));
                        orderProcessed = true;
                    }
                    break;
            }
            if (orderProcessed)
            {
                Log.Debug($"Market order processed");
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
                Log.Warn($"Unable to place order: {order.ToFormattedString()}");
                return null;
            }
        }

        private bool ProcessLimitOrder(Order order, MarketDataPoint currentMarketDataPoint)
        {
            Log.Debug($"Processing limit order {order}");
            var orderProcessed = false;
            switch (order.Side)
            {
                case OrderSide.Buy:
                    if (order.Price >= currentMarketDataPoint.Price)
                    {
                        if (Portfolio.UnfreezeSecurity(order.BaseCurrency, order.Volume * order.Price * (1 + _TradeFee)) &&
                            Portfolio.RemoveSecurity(order.BaseCurrency, order.Volume * currentMarketDataPoint.Price * (1 + _TradeFee)))
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
                            var transactionAmount = order.Volume * currentMarketDataPoint.Price * (1 - _TradeFee);
                            Portfolio.AddSecurity(order.BaseCurrency, transactionAmount);
                            orderProcessed = true;
                        }
                    }
                    break;
            }

            if (orderProcessed)
            {
                Log.Debug($"Limit order processed");
                _PendingOrders.Remove(order);
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
                Log.Warn($"Cannot place order: {order.ToFormattedString()}");
            }

            return orderProcessed;
        }
    }
}
