using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
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

        public GroupListShiftCategoryBackToLegalStateService(
            ISchedulingResultStateHolder stateHolder,
            IScheduleMatrixValueCalculatorProFactory scheduleMatrixValueCalculatorProFactory, 
            IScheduleFairnessCalculator fairnessCalculator, 
            IGroupSchedulingService scheduleService,
            IScheduleDayChangeCallback scheduleDayChangeCallback)
        {
            _stateHolder = stateHolder;
            _scheduleMatrixValueCalculatorProFactory = scheduleMatrixValueCalculatorProFactory;
            _fairnessCalculator = fairnessCalculator;
            _scheduleService = scheduleService;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
        }

        public void Execute(IList<IScheduleMatrixPro> scheduleMatrixList, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences)
        {
        }

        public virtual IGroupShiftCategoryBackToLegalStateServiceBuilder CreateSchedulePeriodBackToLegalStateServiceBuilder(
            IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator)
        {
            return new GroupShiftCategoryBackToLegalStateServiceBuilder(
                scheduleMatrixValueCalculator,
                _scheduleService
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