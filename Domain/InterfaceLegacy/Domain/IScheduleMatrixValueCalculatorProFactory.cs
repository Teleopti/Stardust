using System.Collections.Generic;

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
	    /// <returns></returns>
	    IScheduleMatrixValueCalculatorPro CreateScheduleMatrixValueCalculatorPro(IEnumerable<DateOnly> scheduleDays, IOptimizationPreferences optimizerPreferences, ISchedulingResultStateHolder stateHolder);
    }
}