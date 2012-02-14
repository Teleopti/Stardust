using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Factory class to create objects in the resource optimization area.
    /// </summary>
    public interface IResourceOptimizationFactory
    {



        /// <summary>
        /// Creates the schedule matrix value calculator pro.
        /// </summary>
        /// <param name="scheduleDays">The schedule days.</param>
        /// <param name="optimizerPreferences">The optimizer preferences.</param>
        /// <param name="stateHolder">The state holder.</param>
        /// <param name="fairnessCalculator">The fairness calculator.</param>
        /// <param name="activeSkills">The active skills.</param>
        /// <returns></returns>
        IScheduleMatrixValueCalculatorPro CreateScheduleMatrixValueCalculatorPro(
            IEnumerable<DateOnly> scheduleDays,
            IOptimizerOriginalPreferences optimizerPreferences,
            ISchedulingResultStateHolder stateHolder,
            IScheduleFairnessCalculator fairnessCalculator,
            IList<ISkill> activeSkills);

    }
}