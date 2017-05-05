using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Schedule a person on a day
	/// </summary>
	public interface IScheduleService
	{

		ReadOnlyCollection<WorkShiftFinderResult> FinderResults { get; }

	    bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            SchedulingOptions schedulingOptions,
			IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
            ISchedulePartModifyAndRollbackService rollbackService);

		/// <summary>
		/// Clears the finder results.
		/// </summary>
	    void ClearFinderResults();


		bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            SchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulePartModifyAndRollbackService rollbackService);

	}
}