﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Midgard.Collections
{
    public class ObservableGroupCollection<TKey, TElement> : ReadOnlyObservableCollection<ObservableGroupCollection<TKey, TElement>.ObserableGroup> where TKey : IComparable<TKey>
    {
        private readonly Func<TElement, TKey> selector;
        private readonly ObservableCollection<TElement> baseCollection;
        private readonly Dictionary<TKey, ObserableGroup> groupLookup = new Dictionary<TKey, ObserableGroup>();
        private readonly IComparer<TElement> elementOrder;
        private readonly ObservableCollection<ObserableGroup> storageCollection;


        private ObservableGroupCollection(ObservableCollection<ObserableGroup> storageCollection, ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector, IComparer<TElement> elementOrder) : base(storageCollection)
        {
            this.storageCollection = storageCollection;
            this.elementOrder = elementOrder;
            this.selector = selector;
            this.baseCollection = baseCollection;

            this.baseCollection.CollectionChanged += BaseCollection_CollectionChanged;
            ReInitiliseCollection();
        }

        public static ObservableGroupCollection<TKey, TElement> Create(ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector, IComparer<TElement> elementOrder)
        {
            var backingStore = new ObservableCollection<ObserableGroup>();
            return new ObservableGroupCollection<TKey, TElement>(backingStore, baseCollection, selector, elementOrder);
        }
        public static ObservableGroupCollection<TKey, TElement> Create(ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector, Func<TElement, IComparable> elementOrder)
        {
            return Create(baseCollection, selector, new Comparer(elementOrder));
        }

        public static ObservableGroupCollection<TKey, TElement> Create<TComparable>(ObservableCollection<TElement> baseCollection, Func<TElement, TKey> selector) where TComparable : TElement, IComparable
        {
            return Create(baseCollection, selector, new Comparer((x => (IComparable)x)));
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

            var newGroup = ObserableGroup.Create(key);
            this.groupLookup[key] = newGroup;

            var insertionIndex = this.BinarySearch(newGroup);

            if (insertionIndex < 0)
                insertionIndex = ~insertionIndex;

            this.storageCollection.Insert(insertionIndex, newGroup);

            return newGroup;
        }

        private class Comparer : IComparer<TElement>
        {
            private Func<TElement, IComparable> elementOrder;

            public Comparer(Func<TElement, IComparable> elementOrder) =>
                this.elementOrder = elementOrder;

            public int Compare(TElement x, TElement y) => this.elementOrder(x).CompareTo(this.elementOrder(y));
        }

        public class ObserableGroup : ReadOnlyObservableCollection<TElement>, IGrouping<TKey, TElement>, IComparable<ObserableGroup>
        {
            public TKey Key { get; }

            internal ObservableCollection<TElement> Values { get; }

            private ObserableGroup(TKey key, ObservableCollection<TElement> values) : base(values)
            {
                Key = key;
                Values = values;
            }
            internal static ObserableGroup Create(TKey key)
            {
                var collection = new ObservableCollection<TElement>();
                return new ObserableGroup(key, collection);
            }

            int IComparable<ObserableGroup>.CompareTo(ObserableGroup other) => Key.CompareTo(other.Key);
        }
    }
}
