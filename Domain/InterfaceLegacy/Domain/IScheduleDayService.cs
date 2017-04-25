using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for ScheduleDayService
    /// </summary>
    public interface IScheduleDayService
    {

		/// <summary>
		/// Schedules the day.
		/// </summary>
		/// <param name="schedulePart">The schedule part.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
		bool ScheduleDay(IScheduleDay schedulePart, SchedulingOptions schedulingOptions);

		/// <summary>
		/// Deletes the main shift on supplied ScheduleParts.
		/// </summary>
		/// <param name="schedulePartList">The schedule part list.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
		IList<IScheduleDay> DeleteMainShift(IList<IScheduleDay> schedulePartList, SchedulingOptions schedulingOptions);
    }
}