using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Collections
{
    /// <summary>
    /// Collection that filters on Type
    /// It will only filter the view of the collection, not the collection itself
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 3/13/2008
    /// </remarks>
    public class FilteredCollection<T> : ObservableCollection<T>
    {
        private readonly IList<Type> _types;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilteredCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 3/14/2008
        /// </remarks>
        public FilteredCollection()
            : base()
        {
            _types = new List<Type>();
        }

        /// <summary>
        /// Adds a filter to the definition
        /// </summary>
        /// <param name="type">The type.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 3/13/2008
        /// </remarks>
        public void AddFilter(Type type)
        {
            CollectionViewSource.GetDefaultView(this).Filter = null;
            if (!_types.Contains(type))
            {
                _types.Add(type);
                CollectionViewSource.GetDefaultView(this).Filter = FilterOutType;
            }
        }

        /// <summary>
        /// Removes a type to the filter
        /// </summary>
        /// <param name="type">The type.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 3/13/2008
        /// </remarks>
        public void RemoveFilter(Type type)
        {
            CollectionViewSource.GetDefaultView(this).Filter = null;

            if (_types.IndexOf(type) != -1)
            {
                _types.Remove(type);
                if (_types.Count != 0) CollectionViewSource.GetDefaultView(this).Filter = FilterOutType;

            }
        }

        /// <summary>
        /// Gets the filters.
        /// </summary>
        /// <value>The filters.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 3/13/2008
        /// </remarks>
        public IList<Type> Filters
        {
            get { return _types; }
        }

        protected virtual bool FilterOutType(object item)
        {
            if (_types.Contains(item.GetType())) return false;
            return true;
        }

        /// <summary>
        /// Sets the filter and removes all the other filters
        /// </summary>
        /// <param name="type">The type.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-10-07
        /// </remarks>
        public void SetFilter(Type type)
        {
            Filters.Clear();
            AddFilter(type);
        }

    }

}