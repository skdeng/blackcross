using System.Collections.Generic;

namespace BlackCross.Core.Extensions
{
    public static class LinkedListExtension
    {
        public static void Slide<T>(this LinkedList<T> list, T newVal, int windowSize)
        {
            list.AddLast(newVal);
            if (list.Count > windowSize)
            {
                list.RemoveFirst();
            }
        }
    }
}
