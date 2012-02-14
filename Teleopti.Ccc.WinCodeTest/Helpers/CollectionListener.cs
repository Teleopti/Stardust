﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.WinCodeTest.Helpers
{
    /// <summary>
    /// Helper for observing collections in tests
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CollectionListener<T>
    {
        private readonly ObservableCollection<T> _targetCollection;

        public IList<T> AddedItems { get; private set; }
        public IList<T> RemovedItems { get; private set; }
        public int HasChanged { get; private set; }

        public CollectionListener(ObservableCollection<T> targetCollection)
        {
            AddedItems = new List<T>();
            RemovedItems = new List<T>();
            _targetCollection = targetCollection;
            _targetCollection.CollectionChanged += ((sender, e) =>
                                                        {

                                                            if (e.NewItems != null)
                                                                foreach (var item in e.NewItems)
                                                                {
                                                                    AddedItems.Add((T)item);
                                                                }
                                                            if (e.OldItems != null)
                                                                foreach (var item in e.OldItems)
                                                                {
                                                                    RemovedItems.Add((T)item);
                                                                }
                                                            HasChanged++;

                                                        });
        }

        public void Clear()
        {
            AddedItems.Clear();
            RemovedItems.Clear();
            HasChanged = 0;
        }

        //Checks the removed and added items and also checks that the collection did not fire any extra changes
        public bool CheckChangedItems(int numberOfAddedItems, int numberOfRemovedItems)
        {
            int added = AddedItems.Count;
            int removed = RemovedItems.Count;
            bool checkAdded = added == numberOfAddedItems;
            bool checkRemoved = removed == numberOfRemovedItems;
            bool checkTotal = (added + removed) == HasChanged;
            return (checkAdded && checkRemoved && checkTotal); 
        }
    }
}
