using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupListShiftCategoryBackToLegalStateService
    {
        void Execute(IList<IScheduleMatrixPro> scheduleMatrixList, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences);
    }

    public class GroupListShiftCategoryBackToLegalStateService : IGroupListShiftCategoryBackToLegalStateService
    {
        private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly IScheduleMatrixValueCalculatorProFactory _scheduleMatrixValueCalculatorProFactory;
        private readonly IScheduleFairnessCalculator _fairnessCalculator;
        private readonly IGroupSchedulingService _scheduleService;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

        public GroupListShiftCategoryBackToLegalStateService(
            ISchedulingResultStateHolder stateHolder,
            IScheduleMatrixValueCalculatorProFactory scheduleMatrixValueCalculatorProFactory, 
            IScheduleFairnessCalculator fairnessCalculator, 
            IGroupSchedulingService scheduleService,
            IScheduleDayChangeCallback scheduleDayChangeCallback,
            IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
        {
            _stateHolder = stateHolder;
            _scheduleMatrixValueCalculatorProFactory = scheduleMatrixValueCalculatorProFactory;
            _fairnessCalculator = fairnessCalculator;
            _scheduleService = scheduleService;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
        }

        public void Execute(IList<IScheduleMatrixPro> scheduleMatrixList, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences)
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
                    scheduleMatrixValueCalculator);

            foreach (var matrix in scheduleMatrixList)
            {
                var schedulePartModifyAndRollbackService =
                    new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
                var groupBackToLegalStateService =
                    groupackToLegalStateServiceBuilder.Build(matrix, schedulePartModifyAndRollbackService);
                groupBackToLegalStateService.Execute(matrix.SchedulePeriod, schedulingOptions, scheduleMatrixList);
            }
        }

        public virtual IGroupShiftCategoryBackToLegalStateServiceBuilder CreateGroupBackToLegalStateServiceBuilder(
            IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator)
        {
            return new GroupShiftCategoryBackToLegalStateServiceBuilder(
                scheduleMatrixValueCalculator,
                _scheduleService,
                _groupPersonBuilderForOptimization
                );
        }

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