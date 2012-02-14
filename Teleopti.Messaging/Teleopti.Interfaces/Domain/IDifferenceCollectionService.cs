using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Calculates the difference between two collections of IEntity data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-05-29
    /// </remarks>
    public interface IDifferenceCollectionService<T> where T : class
    {
        /// <summary>
        /// Returns the difference between two collections
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="actual">The actual.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-29
        /// </remarks>
        IDifferenceCollection<T> Difference(IList<T> original, IList<T> actual);
    }
}