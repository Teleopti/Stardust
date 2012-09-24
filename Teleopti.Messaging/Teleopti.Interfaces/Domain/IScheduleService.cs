using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Schedule a person on a day
	/// </summary>
	public interface IScheduleService
	{
		/// <summary>
		/// Gets the finder results.
		/// </summary>
		/// <value>The finder results.</value>
		ReadOnlyCollection<IWorkShiftFinderResult> FinderResults { get; }

		/// <summary>
		/// Schedules the person on day.
		/// </summary>
		/// <param name="schedulePart">The schedule part.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <param name="useOccupancyAdjustment">if set to <c>true</c> then use occupancy adjustment.</param>
		/// <param name="effectiveRestriction">The effective restriction.</param>
		/// <param name="resourceCalculateDelayer">The resource calculate delayer.</param>
		/// <param name="possibleStartEndCategory"> </param>
		/// <returns></returns>
		bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            ISchedulingOptions schedulingOptions,
            bool useOccupancyAdjustment,
			IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IPossibleStartEndCategory possibleStartEndCategory);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="schedulePart"></param>
		/// <param name="schedulingOptions"></param>
		/// <param name="useOccupancyAdjustment"></param>
		/// <param name="effectiveRestriction"></param>
		/// <param name="resourceCalculateDelayer"></param>
		/// <returns></returns>
		bool SchedulePersonOnDay(
			IScheduleDay schedulePart,
			ISchedulingOptions schedulingOptions,
			bool useOccupancyAdjustment,
			IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer);
	    ///<summary>
	    ///</summary>
	    void ClearFinderResults();

		/// <summary>
		/// Creates the effective restriction and Schedule
		/// </summary>
		/// <param name="schedulePart">The schedule part.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <param name="useOccupancyAdjustment">if set to <c>true</c> then use occupancy adjustment.</param>
		/// <param name="resourceCalculateDelayer">The resource calculate delayer.</param>
		/// <param name="possibleStartEndCategory"> </param>
		/// <returns></returns>
		bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            ISchedulingOptions schedulingOptions,
            bool useOccupancyAdjustment,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IPossibleStartEndCategory possibleStartEndCategory);
	}
}