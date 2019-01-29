using System;
using System.Collections.Generic;
using System.Linq;

namespace BlackCross.Core.Extensions
{
    public static class IEnumerableExtension
    {
        public static decimal StdDeviation(this IEnumerable<decimal> values)
        {
            var avg = values.Average();
            var doubleVal = Math.Sqrt(values.Average(v => Math.Pow((double)v - (double)avg, 2)));

            return (decimal)Math.Round(doubleVal, 4);
        }

        public static bool Reduce<T>(this IEnumerable<T> list, Func<T, T, bool> comparer)
        {
            var prev = list.First();
            for (var i = 1; i < list.Count(); i++)
            {
                if (!comparer(prev, list.ElementAt(i)))
                {
                    return false;
                }
                prev = list.ElementAt(i);
            }

            return true;
        }

        public static bool IsAscending(this IEnumerable<decimal> list)
        {
            return list.Reduce((prev, curr) => curr >= prev);
        }

        public static bool IsDescending(this IEnumerable<decimal> list)
        {
            return list.Reduce((prev, curr) => curr <= prev);
        }

        public static string ToPrettyString<T>(this IEnumerable<T> list)
        {
            return "{" + string.Join(",", list) + "}";
        }
    }
}
