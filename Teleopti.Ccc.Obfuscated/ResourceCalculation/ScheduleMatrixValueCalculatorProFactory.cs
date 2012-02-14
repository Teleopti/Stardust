using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    public class ScheduleMatrixValueCalculatorProFactory : IScheduleMatrixValueCalculatorProFactory
    {
        public IScheduleMatrixValueCalculatorPro CreateScheduleMatrixValueCalculatorPro(
            IEnumerable<DateOnly> scheduleDays,
            IOptimizerOriginalPreferences optimizerPreferences,
            ISchedulingResultStateHolder stateHolder,
            IScheduleFairnessCalculator fairnessCalculator)
        {
            return new ScheduleMatrixValueCalculatorPro
                (scheduleDays,
                 optimizerPreferences,
                 stateHolder,
                 fairnessCalculator);
        }
    }
}
