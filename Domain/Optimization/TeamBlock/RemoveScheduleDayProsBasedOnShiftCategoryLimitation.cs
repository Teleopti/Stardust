using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class RemoveScheduleDayProsBasedOnShiftCategoryLimitation
	{
		private readonly ShiftCategoryWeekRemover _shiftCategoryWeekRemover;
		private readonly ShiftCategoryPeriodRemover _shiftCategoryPeriodRemover;

		public RemoveScheduleDayProsBasedOnShiftCategoryLimitation(ShiftCategoryWeekRemover shiftCategoryWeekRemover,
			ShiftCategoryPeriodRemover shiftCategoryPeriodRemover)
		{
			_shiftCategoryWeekRemover = shiftCategoryWeekRemover;
			_shiftCategoryPeriodRemover = shiftCategoryPeriodRemover;
		}

		public IEnumerable<IScheduleDayPro> Execute(SchedulingOptions schedulingOptions, 
			IScheduleMatrixPro scheduleMatrixPro,
			IOptimizationPreferences optimizationPreferences, 
			IShiftCategoryLimitation limitation,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			var removedScheduleDayPros = limitation.Weekly
				? _shiftCategoryWeekRemover.Remove(limitation, schedulingOptions, scheduleMatrixPro, optimizationPreferences, schedulePartModifyAndRollbackService)
				: _shiftCategoryPeriodRemover.RemoveShiftCategoryOnPeriod(limitation, schedulingOptions, scheduleMatrixPro, optimizationPreferences, schedulePartModifyAndRollbackService);
			return removedScheduleDayPros;
		}
	}
}