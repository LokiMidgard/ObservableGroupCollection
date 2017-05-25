﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Midgard
{
    public static class CollectionExtensions
    {

        public static Collections.ObservableGroupCollection<TKey, TElement> AsObservableGrouping<TKey, TElement>(this ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector, IComparer<TElement> elementOrder) where TKey : IComparable<TKey>
        {
            return Collections.ObservableGroupCollection<TKey, TElement>.Create(baseCollection, selector, elementOrder);
        }

        public static Collections.ObservableGroupCollection<TKey, TElement> AsObservableGrouping<TKey, TElement>(this ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector, Func<TElement, IComparable> elementOrder) where TKey : IComparable<TKey>
        {
            return Collections.ObservableGroupCollection<TKey, TElement>.Create(baseCollection, selector, elementOrder);
        }
        public static Collections.ObservableGroupCollection<TKey, TElement> AsObservableGrouping<TKey, TElement>(this ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector)
            where TKey : IComparable<TKey>
            where TElement : IComparable
        {
            return Collections.ObservableGroupCollection<TKey, TElement>.Create<TElement>(baseCollection, selector);
        }


        internal static int BinarySearch<T>(this IList<T> list, T search) where T : IComparable<T>
        {
            var low = 0;
            var high = list.Count - 1;

            while (low <= high)
            {
                var middle = low + ((high - low) >> 1);
                var midValue = list[middle];

                var comparison = search.CompareTo(midValue);
                if (comparison == 0)
                    return middle;

                if (comparison < 0)
                    high = middle - 1;
                else
                    low = middle + 1;
            }

            return ~low;
        }
        internal static int BinarySearch<T>(this IList<T> list, T search, IComparer<T> comparer)
        {
            var low = 0;
            var high = list.Count - 1;

            while (low <= high)
            {
                var middle = low + ((high - low) >> 1);
                var midValue = list[middle];

                var comparison = comparer.Compare(search, midValue);
                if (comparison == 0)
                    return middle;

                if (comparison < 0)
                    high = middle - 1;
                else
                    low = middle + 1;
            }

            return ~low;
        }

    }
}
