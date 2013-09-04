
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Container class for a <see cref="ISchedulePeriod"/> and a <see cref="IScheduleMatrixPro"/>
    /// </summary>
    public interface IScheduleMatrixSchedulePeriodContainer
    {
        /// <summary>
        /// Gets the contained schedule period.
        /// </summary>
        /// <value>The schedule period.</value>
        IVirtualSchedulePeriod SchedulePeriod { get; }

        /// <summary>
        /// Gets the cantained schedule matrix.
        /// </summary>
        /// <value>The schedule matrix.</value>
        IScheduleMatrixPro ScheduleMatrix { get; }
    }
}