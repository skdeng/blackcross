using System.Collections.Generic;
using System.Linq;

namespace BlackCross.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Trade"/>
    /// </summary>
    public static class TradeExtension
    {
        /// <summary>
        /// Get volume-weighted average price (see https://en.wikipedia.org/wiki/Volume-weighted_average_price)
        /// </summary>
        /// <param name="trades">List of trades</param>
        /// <returns>Volume-weighted average price</returns>
        public static decimal GetVWAP(this IEnumerable<Trade> trades)
        {
            if (trades.Count() > 0)
            {
                return trades.Sum(t => t.ExecutionPrice * t.ExecutionVolume) / trades.Sum(t => t.ExecutionVolume);
            }
            return 0;
        }
    }
}
