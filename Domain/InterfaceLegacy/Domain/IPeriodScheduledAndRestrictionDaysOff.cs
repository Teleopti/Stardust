using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Calculates the number of days off. scheduled and from restrictions
    /// </summary>
    public interface IPeriodScheduledAndRestrictionDaysOff
    {
    	/// <summary>
    	/// Calculateds the days off.
    	/// </summary>
		/// <param name="matrix"> </param>
    	/// <param name="useSchedules">if set to <c>true</c> [use schedules].</param>
    	/// <param name="usePreferences">if set to <c>true</c> [use preferences].</param>
    	/// <param name="useRotations">if set to <c>true</c> [use rotations].</param>
    	/// <returns></returns>
		int CalculatedDaysOff(IScheduleMatrixPro matrix, bool useSchedules, bool usePreferences, bool useRotations);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="scheduleDays"></param>
		/// <param name="useSchedules"></param>
		/// <param name="usePreferences"></param>
		/// <param name="useRotations"></param>
		/// <returns></returns>
		int CalculatedDaysOff(IEnumerable<IScheduleDay> scheduleDays, bool useSchedules, bool usePreferences, bool useRotations);
	}
}