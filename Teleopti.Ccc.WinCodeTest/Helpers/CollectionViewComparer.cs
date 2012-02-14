using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Teleopti.Ccc.WinCodeTest.Helpers
{
    /// <summary>
    /// Helper for comparing a collection with a CollectionView
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-06-11
    /// </remarks>
    public class CollectionViewComparer<T>
    {
        
        private ICollectionView CollectionView { get; set; }

        private CollectionViewComparer()
        {
           
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static CollectionViewComparer<T> CreateComparer(ICollectionView view)
        {
            CollectionViewComparer<T> ret = new CollectionViewComparer<T>();
            ret.CollectionView = view;
            return ret;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static CollectionViewComparer<T> CreateDefaultComparer(IEnumerable<T> target)
        {
            return CreateComparer(CollectionViewSource.GetDefaultView(target));
        }

        /// <summary>
        /// Determines whether the specified list to compare contains elements.
        /// </summary>
        /// <param name="listToCompare">The list to compare.</param>
        /// <returns>
        /// 	<c>true</c> if the specified list to compare contains elements; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-06-11
        /// </remarks>
        public bool ContainsElements(IEnumerable<T> listToCompare)
        {
            foreach (T toCompare in listToCompare)
            {
                if (!CollectionView.Contains(toCompare)) return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether [contains only elements from] [the specified list to compare].
        /// </summary>
        /// <param name="listToCompare">The list to compare.</param>
        /// <returns>
        /// 	<c>true</c> if [contains only elements from] [the specified list to compare]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-06-11
        /// </remarks>
        public bool ContainsOnlyElementsFrom(IEnumerable<T> listToCompare)
        {
            foreach (object obj in CollectionView)
            {
                if (!listToCompare.Contains((T)obj)) return false;
            }
            return true;
        }
    }
}
