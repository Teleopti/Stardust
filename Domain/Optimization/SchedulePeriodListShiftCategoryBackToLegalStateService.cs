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
        private readonly IScheduleMatrixValueCalculatorProFactory _scheduleMatrixValueCalculatorProFactory;
        private readonly IScheduleFairnessCalculator _fairnessCalculator;
    	private readonly IScheduleDayService _scheduleDayService;
        private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
	    private readonly ITeamBlockRemoveShiftCategoryBackToLegalService _teamBlockRemoveShiftCategoryBackToLegalService;

	    public SchedulePeriodListShiftCategoryBackToLegalStateService(
            Func<ISchedulingResultStateHolder> stateHolder,
            IScheduleMatrixValueCalculatorProFactory scheduleMatrixValueCalculatorProFactory, 
            IScheduleFairnessCalculator fairnessCalculator, 
			IScheduleDayService scheduleDayService,
            Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			ITeamBlockRemoveShiftCategoryBackToLegalService teamBlockRemoveShiftCategoryBackToLegalService)
        {
            _stateHolder = stateHolder;
            _scheduleMatrixValueCalculatorProFactory = scheduleMatrixValueCalculatorProFactory;
            _fairnessCalculator = fairnessCalculator;
        	_scheduleDayService = scheduleDayService;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
	        _teamBlockRemoveShiftCategoryBackToLegalService = teamBlockRemoveShiftCategoryBackToLegalService;
        }

		public void Execute(IList<IScheduleMatrixPro> scheduleMatrixList, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IResourceOptimizationHelper resourceOptimizationHelper)
        {
            IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator =
                BuildScheduleMatrixValueCalculator(
                    _scheduleMatrixValueCalculatorProFactory,
                    scheduleMatrixList,
					optimizationPreferences, 
                    _stateHolder(), 
                    _fairnessCalculator);


			for (var i = 0; i < 2; i++)
			{
				foreach (var matrix in scheduleMatrixList)
				{
					var schedulePartModifyAndRollbackService = new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(), new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
					var shiftNudgeDirective = new ShiftNudgeDirective();
					var resourceCalculateDelayer = new ResourceCalculateDelayer(resourceOptimizationHelper, 1, true, schedulingOptions.ConsiderShortBreaks);
					_teamBlockRemoveShiftCategoryBackToLegalService.Execute(matrix.SchedulePeriod, schedulingOptions, scheduleMatrixValueCalculator, matrix, _stateHolder(), schedulePartModifyAndRollbackService, resourceCalculateDelayer, scheduleMatrixList, shiftNudgeDirective); 
				}
			}
        }

        public virtual ISchedulePeriodShiftCategoryBackToLegalStateServiceBuilder CreateSchedulePeriodBackToLegalStateServiceBuilder(
            IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator)
        {
            return new SchedulePeriodShiftCategoryBackToLegalStateServiceBuilder(
                                    scheduleMatrixValueCalculator,
									()=>_scheduleDayService);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public virtual IScheduleMatrixValueCalculatorPro BuildScheduleMatrixValueCalculator
            (IScheduleMatrixValueCalculatorProFactory scheduleMatrixValueCalculatorProFactory,
             IList<IScheduleMatrixPro> scheduleMatrixList,
             IOptimizationPreferences optimizerPreferences,
             ISchedulingResultStateHolder stateHolder,
             IScheduleFairnessCalculator fairnessCalculator)
        {
            IList<DateOnly> days = new List<DateOnly>();
            foreach (IScheduleMatrixPro matrix in scheduleMatrixList)
            {
                foreach (IScheduleDayPro day in matrix.EffectivePeriodDays)
                {
                    days.Add(day.Day);
                }
            }
            return scheduleMatrixValueCalculatorProFactory.CreateScheduleMatrixValueCalculatorPro
                (days, optimizerPreferences, stateHolder, fairnessCalculator);
        }
    }
}
