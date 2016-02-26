using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Creates the IIntradayOptimizer2 optimizers list.
    /// </summary>
    public interface IIntradayOptimizer2Creator
    {
	    /// <summary>
	    /// Creates the list of optimizers.
	    /// </summary>
	    /// <returns></returns>
	    IEnumerable<IIntradayOptimizer2> Create(IEnumerable<IScheduleMatrixOriginalStateContainer> scheduleMatrixContainers,
		    IEnumerable<IScheduleMatrixOriginalStateContainer> workShiftContainers, IOptimizationPreferences optimizerPreferences,
		    IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
    }
}