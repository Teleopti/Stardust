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
		/// Deletes the main shift on supplied ScheduleParts.
		/// </summary>
		/// <param name="schedulePartList">The schedule part list.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
		IList<IScheduleDay> DeleteMainShift(IList<IScheduleDay> schedulePartList, SchedulingOptions schedulingOptions);
    }
}