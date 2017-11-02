using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Removes the days with shift categories that are breaking the rules from a list of schedule periods
	/// so that all the schedule periods gets back to legal state and then schedules those days.
	/// </summary>
	[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveClassicShiftCat_46582)]
	public interface ISchedulePeriodListShiftCategoryBackToLegalStateService
	{
		void Execute(IEnumerable<IScheduleMatrixPro> scheduleMatrixList, SchedulingOptions schedulingOptions);
	}
}
