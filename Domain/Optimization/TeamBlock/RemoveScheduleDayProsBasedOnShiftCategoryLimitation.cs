using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class RemoveScheduleDayProsBasedOnShiftCategoryLimitation
	{
		private readonly IShiftCategoryWeekRemover _shiftCategoryWeekRemover;
		private readonly IShiftCategoryPeriodRemover _shiftCategoryPeriodRemover;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;

		public RemoveScheduleDayProsBasedOnShiftCategoryLimitation(IShiftCategoryWeekRemover shiftCategoryWeekRemover,
			IShiftCategoryPeriodRemover shiftCategoryPeriodRemover, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			_shiftCategoryWeekRemover = shiftCategoryWeekRemover;
			_shiftCategoryPeriodRemover = shiftCategoryPeriodRemover;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
		}

		public IEnumerable<IScheduleDayPro> Execute(ISchedulingOptions schedulingOptions, 
			IScheduleMatrixPro scheduleMatrixPro,
			IOptimizationPreferences optimizationPreferences, 
			IShiftCategoryLimitation limitation)
		{
			var removedScheduleDayPros = limitation.Weekly
				? _shiftCategoryWeekRemover.Remove(limitation, schedulingOptions, scheduleMatrixPro, optimizationPreferences, _schedulePartModifyAndRollbackService)
				: _shiftCategoryPeriodRemover.RemoveShiftCategoryOnPeriod(limitation, schedulingOptions, scheduleMatrixPro, optimizationPreferences, _schedulePartModifyAndRollbackService);
			return removedScheduleDayPros;
		}
	}
}