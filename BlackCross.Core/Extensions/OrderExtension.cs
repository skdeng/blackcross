using System.Collections.Generic;
using System.Linq;

namespace BlackCross.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Order"/>
    /// </summary>
    public static class OrderExtension
    {
        /// <summary>
        /// Get volume-weighted average price (see https://en.wikipedia.org/wiki/Volume-weighted_average_price)
        /// </summary>
        /// <param name="orders">List of orders</param>
        /// <returns>Volume-weighted average price</returns>
        public static decimal GetVWAP(this List<Order> orders)
        {
            return orders.Sum(o => o.Price * o.Volume) / orders.Sum(o => o.Volume);
        }
    }
}
