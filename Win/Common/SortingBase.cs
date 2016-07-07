#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace Teleopti.Ccc.Win.Common
{

    /// <summary>
    /// Represents a SortingBase
    /// </summary>
    public class SortingBase<T> : ISort<T>
    {
        /// <summary>
        /// Sorts the type of the by.
        /// </summary>
        /// <typeparam name="TColumnType">The type of the olumn type.</typeparam>
        /// <param name="dataList">The data list.</param>
        /// <param name="sortingColumn">The sorting column.</param>
        /// <param name="isAscending">if set to <c>true</c> [is ascending].</param>
        /// <returns></returns>
        /// <remarks>
        /// Warning: There is reflection using this method
        /// </remarks>
        internal static IList<T> SortByType<TColumnType>(Collection<T> dataList,
                                                  string sortingColumn,
                                                  bool isAscending)
        {
            var param = Expression.Parameter(typeof(T), "dataItem");
            var mySortExpression = Expression.Lambda<Func<T, TColumnType>>(Expression.Property(param, sortingColumn), param);

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
                                                     (mode == SortingModes.Ascending) };
                sortedList = (IList<T>) sortByTypeMethod.Invoke(null, parameters);
            }

            return sortedList;
        }
    }
}
