using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Find out dependencies in two pair lists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-12-08
    /// </remarks>
    public interface IPairMatrixService<T>
    {
        /// <summary>
        /// Gets the result of first dependencies.
        /// </summary>
        /// <value>The first dependencies.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-12-08
        /// </remarks>
        IEnumerable<T> FirstDependencies { get; }

        /// <summary>
        /// Gets the result of second dependencies.
        /// </summary>
        /// <value>The second dependencies.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-12-08
        /// </remarks>
        IEnumerable<T> SecondDependencies { get; }
        
        /// <summary>
        /// Creates the dependencies.
        /// </summary>
        /// <param name="pairList">The pair list.</param>
        /// <param name="entriesForFirst">The entries for first.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-12-08
        /// </remarks>
        void CreateDependencies(IEnumerable<IPair<T>> pairList,IEnumerable<T> entriesForFirst);
    }
}