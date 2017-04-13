#region Imports

using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Defines the functionality of a ISort<T>
    /// </summary>
    public interface ISort<T>
    {

        #region Properties - Instance Member

        #endregion

        #region Events - Instance Member

        #endregion

        #region Methods - Instance Member

        /// <summary>
        /// Sorts the specified column list.
        /// </summary>
        /// <param name="column">The column list.</param>
        /// <param name="collection">The collection.</param>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-08-19
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IList<T> Sort(ISortColumn<T> column,
                      ReadOnlyCollection<T> collection,
                      SortingModes mode);

        #endregion

    }

    /// <summary>
    /// Sorting delegate
    /// </summary>
    public delegate int SortCompare<T>(T left, T right);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by:VirajS
    /// Created date: 2008-08-19
    /// </remarks>
    public class ComparerBase<T> : IComparer<T>
    {
        /// <summary>
        /// SortCompare delegate instance
        /// </summary>
        private SortCompare<T> sortCompare = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparerBase&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="sortCompare">The sort compare.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-08-19
        /// </remarks>
        public ComparerBase(SortCompare<T> sortCompare)
        {
            this.sortCompare = sortCompare;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value Condition Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-08-19
        /// </remarks>
        public int Compare(T x, T y)
        {
            return sortCompare(x, y);
        }
    }
}
