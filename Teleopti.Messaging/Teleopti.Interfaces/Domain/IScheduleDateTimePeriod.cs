using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Gives support to the scheduler on what needed to load, what to show etc
    /// based on a period choosen by the user
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-05-14
    /// </remarks>
    public interface IScheduleDateTimePeriod
    {
        /// <summary>
        /// Gets the person collection.
        /// </summary>
        /// <value>The person collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-14
        /// </remarks>
        ReadOnlyCollection<IPerson> PersonCollection { get; }

        /// <summary>
        /// Gets the visible period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-14
        /// </remarks>
        DateTimePeriod VisiblePeriod { get; }

        /// <summary>
        /// Gets or sets the range to load calculator.
        /// </summary>
        /// <value>The range to load calculator.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-20
        /// </remarks>
        ISchedulerRangeToLoadCalculator RangeToLoadCalculator { get; }

        /// <summary>
        /// Gets the max period, based on earliest and latest date
        /// from the people's scheduleperiod
        /// </summary>
        /// <value>The max period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-14
        /// </remarks>
        DateTimePeriod LoadedPeriod();
    }
}