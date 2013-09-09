using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds a collection of collection items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-08-25
    /// </remarks>
    public interface IDifferenceCollection<T> : IEnumerable<DifferenceCollectionItem<T>> where T : class
    {
        /// <summary>
        /// Finds the item by original id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-11-13
        /// </remarks>
        DifferenceCollectionItem<T>? FindItemByOriginalId(Guid id);

        DifferenceCollectionItem<T>? FindItemByMatchingOriginal(T match);
    }
}
