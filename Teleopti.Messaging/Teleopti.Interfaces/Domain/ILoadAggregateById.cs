using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Load a complete aggregate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-06-25
    /// </remarks>
    public interface ILoadAggregateById<T>
    {
        /// <summary>
        /// Loads the aggregate.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-06-25
        /// </remarks>
        T LoadAggregate(Guid id);
    }
}