using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Swap service new
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public interface ISwapServiceNew
	{
		/// <summary>
		/// Inits the specified selected schedules.
		/// </summary>
		/// <param name="selectedSchedules">The selected schedules.</param>
		void Init(IList<IScheduleDay> selectedSchedules);

		/// <summary>
		/// Determines whether it is legal to swap assignments between specified selected schedules.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance can swap assignments; otherwise, <c>false</c>.
		/// </returns>
		bool CanSwapAssignments();


		/// <summary>
		/// Swaps two assignments if permitted.
		/// </summary>
		/// <returns></returns>
		IList<IScheduleDay> Swap(IScheduleDictionary schedules, TrackedCommandInfo trackedCommandInfo = null);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="selectedSchedules"></param>
		/// <param name="schedules"></param>
		/// <returns></returns>
		IList<IScheduleDay> Swap(IList<IScheduleDay> selectedSchedules, IScheduleDictionary schedules);
	}
}
