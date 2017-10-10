using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Schedule a person on a day
	/// </summary>
	public interface IScheduleService
	{

	    bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            SchedulingOptions schedulingOptions,
			IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
            ISchedulePartModifyAndRollbackService rollbackService);

		bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            SchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulePartModifyAndRollbackService rollbackService);

	}
}