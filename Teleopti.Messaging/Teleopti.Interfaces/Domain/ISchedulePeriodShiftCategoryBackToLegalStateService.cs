namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Removes the days with shift categories that are breaking the rules from the schedule period
    /// so that the schedule period gets back to legal state and then schedules those days.
    /// </summary>
    public interface ISchedulePeriodShiftCategoryBackToLegalStateService
    {
        /// <summary>
        /// Executes the main task.
        /// </summary>
        /// <param name="schedulePeriod">The schedule period.</param>
        /// <returns></returns>
        bool Execute(IVirtualSchedulePeriod schedulePeriod);
    }
}