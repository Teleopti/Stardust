using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Removes the days with shift categories that are breaking the rules from a list of schedule periods
    /// so that all the schedule periods gets back to legal state and then schedules those days.
    /// </summary>
    public interface ISchedulePeriodListShiftCategoryBackToLegalStateService
    {
        /// <summary>
        /// Executes the back to legal state for a list of <see cref="ISchedulePeriod"/>s.
        /// </summary>
        /// <returns></returns>
        void Execute(IList<IScheduleMatrixPro> scheduleMatrixList);
    }
}