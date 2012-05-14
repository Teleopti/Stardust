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
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
		bool RescheduleDay(IScheduleDay schedulePart, ISchedulingOptions schedulingOptions);

		/// <summary>
		/// Schedules the day.
		/// </summary>
		/// <param name="schedulePart">The schedule part.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
		bool ScheduleDay(IScheduleDay schedulePart, ISchedulingOptions schedulingOptions);

		/// <summary>
		/// Deletes the main shift on supplied ScheduleParts.
		/// </summary>
		/// <param name="schedulePartList">The schedule part list.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
		IList<IScheduleDay> DeleteMainShift(IList<IScheduleDay> schedulePartList, ISchedulingOptions schedulingOptions);
    }
}