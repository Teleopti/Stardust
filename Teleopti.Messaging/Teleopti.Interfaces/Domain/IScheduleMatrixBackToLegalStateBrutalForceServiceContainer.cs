using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Container class to hold an instance of <see cref="ISchedulePeriodDayOffBackToLegalStateByBrutalForceService"/>
    /// and a <see cref="IScheduleMatrixPro"/>
    /// </summary>
    public interface IScheduleMatrixBackToLegalStateBrutalForceServiceContainer
    {
        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <value>The service.</value>
        ISchedulePeriodDayOffBackToLegalStateByBrutalForceService Service { get; }

        /// <summary>
        /// Gets the schedule matrix.
        /// </summary>
        /// <value>The schedule matrix.</value>
        IScheduleMatrixPro ScheduleMatrix { get; }
    }
}