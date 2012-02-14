#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Teleopti.Ccc.Win.Common.Controls.Columns;

#endregion

namespace Teleopti.Ccc.Win.Common
{

    /// <summary>
    /// Represents a SortingBase
    /// </summary>
    public class SortingBase<T> : ISort<T>
    {

        #region Fields - Instance Member

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - SortingBase Members

        #endregion

        #endregion

        #region Events - Instance Member

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - SortingBase Members

        /// <summary>
        /// Sorts the type of the by.
        /// </summary>
        /// <typeparam name="ColumnType">The type of the olumn type.</typeparam>
        /// <param name="dataList">The data list.</param>
        /// <param name="sortingColumn">The sorting column.</param>
        /// <param name="isAscending">if set to <c>true</c> [is ascending].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-08-19
        /// </remarks>
        internal static IList<T> SortByType<ColumnType>(Collection<T> dataList,
                                                  string sortingColumn,
                                                  bool isAscending)
        {
            var param = Expression.Parameter(typeof(T), "dataItem");
            var mySortExpression = Expression.Lambda<Func<T, ColumnType>>(Expression.Property(param, sortingColumn), param);

            List<T> result;
            IQueryable<T> queryableList = dataList.AsQueryable();

            if (isAscending)
            {
                result = queryableList.OrderBy(mySortExpression.Compile()).ToList();
            }
            else
            {
                result = queryableList.OrderByDescending(mySortExpression.Compile()).ToList();
            }
            return result;
        }

        #endregion

        #endregion

        #region ISort<T> Members

        /// <summary>
        /// Sorts the specified column list.
        /// </summary>
        /// <param name="column">The selected olumn.</param>
        /// <param name="collection">The collection.</param>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-08-19
        /// </remarks>
        public IList<T> Sort(ISortColumn<T> column,
                             ReadOnlyCollection<T> collection,
                             SortingModes mode)
        {
            IList<T> sortedList;
            List<T> adapterList = new List<T>(collection);

            string sortColumn = column.BindingProperty;
            IComparer<T> comparer = column.Comparer;

            if (comparer != null)
            {
                adapterList.Sort(comparer);
                if (mode == SortingModes.Descending)
                    adapterList.Reverse();

                sortedList = adapterList;
            }
            else
            {
                MethodInfo sortByTypeMethod = typeof(SortingBase<T>).GetMethod("SortByType", 
                                                            BindingFlags.Static | BindingFlags.NonPublic);
                PropertyInfo sortingColumnProperty = typeof(T).GetProperty(sortColumn);
                Type sortingColumnType = sortingColumnProperty.PropertyType;
                Type[] types = new Type[] { sortingColumnType };
                sortByTypeMethod = sortByTypeMethod.MakeGenericMethod(types);
                object[] parameters = new object[] { new Collection<T>(adapterList), 
                                                     sortColumn, 
                                                     (mode == SortingModes.Ascending) ? true : false };
                sortedList = (IList<T>) sortByTypeMethod.Invoke(null, parameters);
            }

            return sortedList;
        }

        #endregion
    }
}
