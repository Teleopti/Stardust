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
		/// <param name="effectiveRestriction">The effective restriction.</param>
		/// <param name="resourceCalculateDelayer">The resource calculate delayer.</param>
		/// <param name="possibleStartEndCategory">The possible start end category.</param>
		/// <param name="rollbackService">The rollback service.</param>
		/// <returns></returns>
	    bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            ISchedulingOptions schedulingOptions,
			IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IPossibleStartEndCategory possibleStartEndCategory,
            ISchedulePartModifyAndRollbackService rollbackService);


		/// <summary>
		/// Clears the finder results.
		/// </summary>
	    void ClearFinderResults();

		/// <summary>
		/// Creates the effective restriction and Schedule
		/// </summary>
		/// <param name="schedulePart">The schedule part.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <param name="resourceCalculateDelayer">The resource calculate delayer.</param>
		/// <param name="possibleStartEndCategory">The possible start end category.</param>
		/// <param name="rollbackService">The rollback service.</param>
		/// <returns></returns>
		bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            ISchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IPossibleStartEndCategory possibleStartEndCategory,
			ISchedulePartModifyAndRollbackService rollbackService);

        //bool ScheduleForAdvanceScheduling(
        //    IScheduleDay schedulePart,
        //    ISchedulingOptions schedulingOptions,
        //    IResourceCalculateDelayer resourceCalculateDelayer,
        //    ISchedulePartModifyAndRollbackService rollbackService,
        //    IShiftProjectionCache shiftProjectionCache);
	}
}