using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class SchedulePeriodListShiftCategoryBackToLegalStateService : ISchedulePeriodListShiftCategoryBackToLegalStateService
    {
        private readonly Func<ISchedulingResultStateHolder> _stateHolder;
        private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
	    private readonly ITeamBlockRemoveShiftCategoryBackToLegalService _teamBlockRemoveShiftCategoryBackToLegalService;

	    public SchedulePeriodListShiftCategoryBackToLegalStateService(
            Func<ISchedulingResultStateHolder> stateHolder,
            Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			ITeamBlockRemoveShiftCategoryBackToLegalService teamBlockRemoveShiftCategoryBackToLegalService)
        {
            _stateHolder = stateHolder;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
	        _teamBlockRemoveShiftCategoryBackToLegalService = teamBlockRemoveShiftCategoryBackToLegalService;
        }

		public void Execute(IList<IScheduleMatrixPro> scheduleMatrixList, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IResourceOptimizationHelper resourceOptimizationHelper)
        {
			var schedulePartModifyAndRollbackService = new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(), new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			var shiftNudgeDirective = new ShiftNudgeDirective();
			var resourceCalculateDelayer = new ResourceCalculateDelayer(resourceOptimizationHelper, 1, true, schedulingOptions.ConsiderShortBreaks);

			for (var i = 0; i < 2; i++)
			{
				foreach (var matrix in scheduleMatrixList)
				{
					_teamBlockRemoveShiftCategoryBackToLegalService.Execute(schedulingOptions, matrix, _stateHolder(), schedulePartModifyAndRollbackService, resourceCalculateDelayer, scheduleMatrixList, shiftNudgeDirective, optimizationPreferences); 
				}
			}
        }
    }
}
