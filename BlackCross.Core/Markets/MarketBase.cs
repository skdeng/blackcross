using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace BlackCross.Core.Markets
{
    /// <summary>
    /// Base class for market terminal, can be used to both get market data and perface market operations like submitting orders
    /// </summary>
    public abstract class MarketBase
    {
        /// <summary>
        /// Current market's datafeed
        /// </summary>
        public MarketDataFeedBase DataFeed { get; protected set; }

        /// <summary>
        /// Name of the market
        /// </summary>
        public readonly string MarketName;

        /// <summary>
        /// Portfolio
        /// </summary>
        public Portfolio Portfolio { get; protected set; }

        /// <summary>
        /// Event to trigger after a trade has executed
        /// </summary>
        public event EventHandler<Trade> TradeExecuted;

        /// <summary>
        /// Event to trigger after an order has been cancelled, will only be called if the order has been cancelled successfully
        /// </summary>
        public event EventHandler<Order> OrderCancelled;

        /// <summary>
        /// Event to trigger after an order has been opened, will only be called if the order has been executed successfully
        /// </summary>
        public event EventHandler<Order> OrderOpened;

        /// <summary>
        /// Logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="marketName">Name of the market, used for reporting</param>
        public MarketBase(string marketName)
        {
            Portfolio = new Portfolio();
            MarketName = marketName;
        }

        /// <summary>
        /// Submit and open order request
        /// </summary>
        /// <param name="order">Order to open</param>
        /// <returns>Order object that has been opened if the operation succeeded, null otherwise</returns>
        public abstract Order OpenOrder(Order order);

        /// <summary>
        /// Cancel all opened orders
        /// </summary>
        /// <returns>List of orders that has not been cancelled successfully</returns>
        public abstract IEnumerable<Order> CancelAllOrders();

        /// <summary>
        /// Cancel a specific open order
        /// </summary>
        /// <param name="order">Order to cancel</param>
        /// <returns>true if the order has been cancelled successfully, false otherwise</returns>
        public abstract bool CancelOrder(Order order);

        /// <summary>
        /// Cancel a specific open order
        /// </summary>
        /// <param name="clientOrderId">Client order ID of the order to cancel</param>
        /// <returns>true if the order has been cancelled successfully, false otherwise</returns>
        public abstract bool CancelOrder(string clientOrderId);

        /// <summary>
        /// Get the current price all tracking securities
        /// </summary>
        /// <returns></returns>
        public abstract MarketDataFrame GetLastPrice();

        public abstract MarketDataFrame Tick(TimeSpan? tickInterval = null);

        /// <summary>
        /// Get the total return for a given security since the market object has been created
        /// </summary>
        /// <param name="security">Security to track</param>
        /// <returns>Return in decimal (10% return => 1.1)</returns>
        public abstract decimal GetBenchmarkReturn(string security);

        /// <summary>
        /// Helper to execute TradeExecuted event on a separate thread
        /// </summary>
        /// <param name="trade">Trade that was executed</param>
        /// <returns>Task being executed on. This allows the caller to await the call</returns>
        protected Task OnTradeExecuted(Trade trade)
        {
            return Task.Run(() => TradeExecuted?.Invoke(this, trade));
        }

        /// <summary>
        /// Helper to execute OrderCancelled event on a separate thread
        /// </summary>
        /// <param name="order">Order that was cancelled</param>
        /// <returns>Task being executed on. This allows the caller to await the call</returns>
        protected Task OnOrderCancelled(Order order)
        {
            return Task.Run(() => OrderCancelled?.Invoke(this, order));
        }

        /// <summary>
        /// Helper to execute OrderOpened event on a separate thread
        /// </summary>
        /// <param name="order">Order that was opened</param>
        /// <returns>Task being executed on. This allows the caller to await the call</returns>
        protected Task OnOrderOpened(Order order)
        {
            return Task.Run(() => OrderOpened?.Invoke(this, order));
        }
    }
}
