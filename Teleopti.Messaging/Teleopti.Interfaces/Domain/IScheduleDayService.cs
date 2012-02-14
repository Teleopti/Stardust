using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for ScheduleDayService
    /// </summary>
    public interface IScheduleDayService
    {
        /// <summary>
        /// Reschedule one day for a person.
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <returns></returns>
        bool RescheduleDay(IScheduleDay schedulePart);

        /// <summary>
        /// Schedules the day.
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <returns></returns>
        bool ScheduleDay(IScheduleDay schedulePart);

        /// <summary>
        /// Deletes the main shift on supplied ScheduleParts.
        /// </summary>
        /// <param name="schedulePartList">The schedule part list.</param>
        /// <returns></returns>
        IList<IScheduleDay> DeleteMainShift(IList<IScheduleDay> schedulePartList);
    }
}