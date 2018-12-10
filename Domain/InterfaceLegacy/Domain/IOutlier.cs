using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Represents a special event for one workload.
    /// </summary>
    public interface IOutlier : ICloneableEntity<IOutlier>, IAggregateRoot
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-15
        /// </remarks>
        Description Description { get; set; }

        /// <summary>
        /// Gets the workload.
        /// </summary>
        /// <value>The workload.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        IWorkload Workload { get; }

        /// <summary>
        /// Gets all dates.
        /// </summary>
        /// <value>All dates.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        IList<DateOnly> Dates { get; }


        /// <summary>
        /// Gets the dates.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        IList<DateOnly> GetDatesByPeriod(DateOnlyPeriod period);

        /// <summary>
        /// Adds the date.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        void AddDate(DateOnly dateTime);

        /// <summary>
        /// Removes the date.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        void RemoveDate(DateOnly dateTime);
    }
}