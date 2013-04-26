using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupListShiftCategoryBackToLegalStateService
    {
        void Execute(IList<IScheduleMatrixPro> scheduleMatrixList,
            ISchedulingOptions schedulingOptions,
            IOptimizationPreferences optimizationPreferences, 
            IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup,
			ITeamSteadyStateHolder teamSteadyStateHolder,
			ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler,
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization);
    }

    public class GroupListShiftCategoryBackToLegalStateService : IGroupListShiftCategoryBackToLegalStateService
    {
        private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly IScheduleMatrixValueCalculatorProFactory _scheduleMatrixValueCalculatorProFactory;
        private readonly IScheduleFairnessCalculator _fairnessCalculator;
        private readonly IGroupSchedulingService _scheduleService;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private readonly IGroupPersonsBuilder _groupPersonsBuilder;

        public GroupListShiftCategoryBackToLegalStateService(
            ISchedulingResultStateHolder stateHolder,
            IScheduleMatrixValueCalculatorProFactory scheduleMatrixValueCalculatorProFactory, 
            IScheduleFairnessCalculator fairnessCalculator, 
            IGroupSchedulingService scheduleService,
            IScheduleDayChangeCallback scheduleDayChangeCallback,IGroupPersonsBuilder groupPersonsBuilder)
        {
            _stateHolder = stateHolder;
            _scheduleMatrixValueCalculatorProFactory = scheduleMatrixValueCalculatorProFactory;
            _fairnessCalculator = fairnessCalculator;
            _scheduleService = scheduleService;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
            _groupPersonsBuilder = groupPersonsBuilder;
        }

        public void Execute(IList<IScheduleMatrixPro> scheduleMatrixList,
            ISchedulingOptions schedulingOptions,
            IOptimizationPreferences optimizationPreferences,
			IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup, 
			ITeamSteadyStateHolder teamSteadyStateHolder, 
			ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler,
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
        {
            var scheduleMatrixValueCalculator =
                BuildScheduleMatrixValueCalculator(
                    _scheduleMatrixValueCalculatorProFactory,
                    scheduleMatrixList,
                    optimizationPreferences,
                    _stateHolder,
                    _fairnessCalculator);

            var groupackToLegalStateServiceBuilder =
                CreateGroupBackToLegalStateServiceBuilder(
                    scheduleMatrixValueCalculator, groupOptimizerFindMatrixesForGroup);

            var runningList = new List<IScheduleMatrixPro>();
            runningList.AddRange(scheduleMatrixList);

            while(runningList.Count> 0)
            {
                var removeList = new List<IScheduleMatrixPro>();
                foreach (var matrix in runningList)
                {
                    var schedulePartModifyAndRollbackService =
                   new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback,
                                                            new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
                    var groupBackToLegalStateService =
                        groupackToLegalStateServiceBuilder.Build(matrix, schedulePartModifyAndRollbackService);
					var toRemove = groupBackToLegalStateService.Execute(matrix.SchedulePeriod, schedulingOptions, scheduleMatrixList, groupOptimizerFindMatrixesForGroup, teamSteadyStateHolder, teamSteadyStateMainShiftScheduler, _stateHolder.Schedules, schedulePartModifyAndRollbackService, groupPersonBuilderForOptimization);
                    foreach (var matrixPro in toRemove)
                    {
                        if (!removeList.Contains(matrixPro))
                            removeList.Add(matrixPro);
                    }
                }
                foreach (var matrixPro in removeList)
                {
                    runningList.Remove(matrixPro);
                }
            }
        }

        public virtual IGroupShiftCategoryBackToLegalStateServiceBuilder CreateGroupBackToLegalStateServiceBuilder(
            IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator, IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup)
        {
            return new GroupShiftCategoryBackToLegalStateServiceBuilder(
                scheduleMatrixValueCalculator,
                _scheduleService,
                _groupPersonsBuilder, groupOptimizerFindMatrixesForGroup
                );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual IScheduleMatrixValueCalculatorPro BuildScheduleMatrixValueCalculator
            (IScheduleMatrixValueCalculatorProFactory scheduleMatrixValueCalculatorProFactory,
             IList<IScheduleMatrixPro> scheduleMatrixList,
             IOptimizationPreferences optimizerPreferences,
             ISchedulingResultStateHolder stateHolder,
             IScheduleFairnessCalculator fairnessCalculator)
        {
            IList<DateOnly> days = new List<DateOnly>();
            if (scheduleMatrixList != null)
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