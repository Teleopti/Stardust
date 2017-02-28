using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Swap service new
	/// </summary>
	public interface ISwapServiceNew
	{
		/// <summary>
		/// Swaps two assignments if permitted.
		/// </summary>
		/// <returns></returns>
		IList<IScheduleDay> Swap(IScheduleDictionary schedules, IList<IScheduleDay> selectedSchedules, TrackedCommandInfo trackedCommandInfo = null);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="selectedSchedules"></param>
		/// <param name="schedules"></param>
		/// <returns></returns>
		IList<IScheduleDay> Swap(IList<IScheduleDay> selectedSchedules, IScheduleDictionary schedules);
	}
}
