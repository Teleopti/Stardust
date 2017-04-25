using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Removes the days with shift categories that are breaking the rules from a list of schedule periods
	/// so that all the schedule periods gets back to legal state and then schedules those days.
	/// </summary>
	public interface ISchedulePeriodListShiftCategoryBackToLegalStateService
	{
		/// <summary>
		/// Executes the back to legal state for a list of <see cref="ISchedulePeriod"/>s.
		/// </summary>
		/// <param name="scheduleMatrixList">The schedule matrix list.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <param name="optimizationPreferences">The optimization preferences.</param>
		void Execute(IList<IScheduleMatrixPro> scheduleMatrixList, SchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences);
	}
}
