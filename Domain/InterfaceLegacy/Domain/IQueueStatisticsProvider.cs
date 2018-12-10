namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Used for calculation of statistics from queue and when transferring those values to the workload days
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2010-03-11
    /// </remarks>
    public interface IQueueStatisticsProvider
    {
        /// <summary>
        /// Gets the statistics for period.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-03-11
        /// </remarks>
        IStatisticTask GetStatisticsForPeriod(DateTimePeriod period);
    }
}