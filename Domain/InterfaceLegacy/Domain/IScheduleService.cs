using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Schedule a person on a day
	/// </summary>
	public interface IScheduleService
	{

		ReadOnlyCollection<IWorkShiftFinderResult> FinderResults { get; }

	    bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            ISchedulingOptions schedulingOptions,
			IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
            ISchedulePartModifyAndRollbackService rollbackService);

		/// <summary>
		/// Clears the finder results.
		/// </summary>
	    void ClearFinderResults();


		bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            ISchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulePartModifyAndRollbackService rollbackService);

	}
}