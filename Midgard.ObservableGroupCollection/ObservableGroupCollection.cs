using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Midgard.Collections
{
    public class ObservableGroupCollection<TKey, TElement> : ReadOnlyObservableCollection<ObservableGroupCollection<TKey, TElement>.ObserableGroup>
    {
        private readonly Func<TElement, TKey> selector;
        private readonly ObservableCollection<TElement> baseCollection;
        private readonly Dictionary<TKey, ObserableGroup> groupLookup = new Dictionary<TKey, ObserableGroup>();
        private readonly IComparer<TElement> elementOrder;
        private readonly ObservableCollection<ObserableGroup> storageCollection;
        private readonly IComparer<TKey> keyOrder;

        private ObservableGroupCollection(ObservableCollection<ObserableGroup> storageCollection, ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector, IComparer<TKey> keyOrder, IComparer<TElement> elementOrder) : base(storageCollection)
        {
            this.storageCollection = storageCollection;
            this.elementOrder = elementOrder;
            this.keyOrder = keyOrder;
            this.selector = selector;
            this.baseCollection = baseCollection;

            this.baseCollection.CollectionChanged += BaseCollection_CollectionChanged;
            ReInitiliseCollection();
        }

        public static ObservableGroupCollection<TKey, TElement> Create(ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector, IComparer<TKey> keyOrder, IComparer<TElement> elementOrder)
        {
            var backingStore = new ObservableCollection<ObserableGroup>();
            return new ObservableGroupCollection<TKey, TElement>(backingStore, baseCollection, selector, keyOrder, elementOrder);
        }
        public static ObservableGroupCollection<TKey, TElement> Create(ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector, Func<TKey, IComparable> keyOrder, Func<TElement, IComparable> elementOrder)
        {
            return Create(baseCollection, selector, new Comparer<TKey>(keyOrder), new Comparer<TElement>(elementOrder));
        }

        public static ObservableGroupCollection<TKey, TElement> Create<TKeyComparable, TElementComparable>(ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector)
            where TKeyComparable : TKey, IComparable
            where TElementComparable : TElement, IComparable
        {
            return Create(baseCollection, selector, new Comparer<TKey>((x => (IComparable)x)), new Comparer<TElement>((x => (IComparable)x)));
        }

        public static ObservableGroupCollection<TKey, TElement> Create<TKeyComparable>(ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector, Func<TElement, IComparable> elementOrder)
            where TKeyComparable : TKey, IComparable
        {
            return Create(baseCollection, selector, new Comparer<TKey>((x => (IComparable)x)), new Comparer<TElement>(elementOrder));
        }
        public static ObservableGroupCollection<TKey, TElement> Create<TKeyComparable>(ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector, IComparer<TElement> elementOrder)
            where TKeyComparable : TKey, IComparable
        {
            return Create(baseCollection, selector, new Comparer<TKey>((x => (IComparable)x)), elementOrder);
        }

        public static ObservableGroupCollection<TKey, TElement> Create<TElementComparable>(ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector, Func<TKey, IComparable> keyOrder)
            where TElementComparable : TElement, IComparable
        {
            return Create(baseCollection, selector, new Comparer<TKey>(keyOrder), new Comparer<TElement>((x => (IComparable)x)));
        }
        public static ObservableGroupCollection<TKey, TElement> Create<TElementComparable>(ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector, IComparer<TKey> keyOrder)
            where TElementComparable : TElement, IComparable
        {
            return Create(baseCollection, selector, keyOrder, new Comparer<TElement>((x => (IComparable)x)));
        }

        private void BaseCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                        foreach (var item in e.NewItems.OfType<TElement>())
                            AddElement(item);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                        foreach (var item in e.OldItems.OfType<TElement>())
                            RemoveElement(item);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                        foreach (var item in e.OldItems.OfType<TElement>())
                            RemoveElement(item);
                    if (e.NewItems != null)
                        foreach (var item in e.NewItems.OfType<TElement>())
                            AddElement(item);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    ReInitiliseCollection();
                    break;
                default:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break; // Ignore move because we have our own order
            }
        }
        private void ReInitiliseCollection()
        {
            this.storageCollection.Clear();
            this.groupLookup.Clear();
            foreach (var item in this.baseCollection)
                AddElement(item);
        }

        private void RemoveElement(TElement item)
        {
            var key = this.selector(item);
            var group = GetOrCreateGrup(key);

            group.Values.Remove(item);
            if (group.Values.Count <= 0)
            {
                // No Items in group Remove Group
                this.groupLookup.Remove(group.Key);
                this.storageCollection.Remove(group);
            }

        }

        private void AddElement(TElement item)
        {
            var key = this.selector(item);
            var group = GetOrCreateGrup(key);

            var insertionIndex = group.Values.BinarySearch(item, this.elementOrder);
            if (insertionIndex < 0)
                insertionIndex = ~insertionIndex;

            group.Values.Insert(insertionIndex, item);
        }

        private ObserableGroup GetOrCreateGrup(TKey key)
        {
            if (this.groupLookup.ContainsKey(key))
                return this.groupLookup[key];

            var newGroup = ObserableGroup.Create(key, this.keyOrder);
            this.groupLookup[key] = newGroup;

            var insertionIndex = this.BinarySearch(newGroup);

            if (insertionIndex < 0)
                insertionIndex = ~insertionIndex;

            this.storageCollection.Insert(insertionIndex, newGroup);

            return newGroup;
        }

        private class Comparer<T> : IComparer<T>
        {
            private Func<T, IComparable> elementOrder;

            public Comparer(Func<T, IComparable> elementOrder) =>
                this.elementOrder = elementOrder;

            public int Compare(T x, T y) => this.elementOrder(x).CompareTo(this.elementOrder(y));
        }

        public class ObserableGroup : ReadOnlyObservableCollection<TElement>, IGrouping<TKey, TElement>, IComparable<ObserableGroup>
        {
            private readonly IComparer<TKey> comparer;

            public TKey Key { get; }

            internal ObservableCollection<TElement> Values { get; }

            private ObserableGroup(TKey key, ObservableCollection<TElement> values, IComparer<TKey> comparer) : base(values)
            {
                Key = key;
                Values = values;
                this.comparer = comparer;
            }
            internal static ObserableGroup Create(TKey key, IComparer<TKey> comparer)
            {
                var collection = new ObservableCollection<TElement>();
                return new ObserableGroup(key, collection, comparer);
            }

            int IComparable<ObserableGroup>.CompareTo(ObserableGroup other) => this.comparer.Compare(Key, other.Key);
        }
    }
}
