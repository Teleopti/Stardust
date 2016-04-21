using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A custom collection of generic type Layer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILayerCollection<T> : IList<ILayer<T>>
    {
        /// <summary>
        /// Get the period with the earliest start and the latest end.
        /// </summary>
        /// <returns></returns>
        DateTimePeriod? Period();

        /// <summary>
        /// Gets the first start.
        /// </summary>
        /// <value>The first start.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-18
        /// </remarks>
        DateTime? FirstStart();

        /// <summary>
        /// Gets the latest end.
        /// </summary>
        /// <value>The latest end.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-18
        /// </remarks>
        DateTime? LatestEnd();
    }
}
