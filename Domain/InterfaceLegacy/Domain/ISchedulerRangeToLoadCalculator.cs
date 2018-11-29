
namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Calculates the loading period for scheduler
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-05-20
    /// </remarks>
    public interface ISchedulerRangeToLoadCalculator
    {
        /// <summary>
        /// Gets the scheduler range to load.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-19
        /// </remarks>
        DateTimePeriod SchedulerRangeToLoad(IPerson person);

        /// <summary>
        /// Gets the requested period.
        /// </summary>
        /// <value>The requested period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-20
        /// </remarks>
        DateTimePeriod RequestedPeriod { get; }
    }
}