using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Factory for <see cref="IScheduleMatrixValueCalculatorPro"/> class.
    /// </summary>
    public interface IScheduleMatrixValueCalculatorProFactory
    {
        /// <summary>
        /// Creates the schedule matrix value calculator pro.
        /// </summary>
        /// <param name="scheduleDays">The schedule days.</param>
        /// <param name="optimizerPreferences">The optimizer preferences.</param>
        /// <param name="stateHolder">The state holder.</param>
        /// <param name="fairnessCalculator">The fairness calculator.</param>
        /// <returns></returns>
        IScheduleMatrixValueCalculatorPro CreateScheduleMatrixValueCalculatorPro(
            IEnumerable<DateOnly> scheduleDays,
            IOptimizerOriginalPreferences optimizerPreferences,
            ISchedulingResultStateHolder stateHolder,
            IScheduleFairnessCalculator fairnessCalculator);
    }
}