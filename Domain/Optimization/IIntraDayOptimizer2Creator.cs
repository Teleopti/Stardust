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
	    IList<IIntradayOptimizer2> Create(IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixContainerList,
		    IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList, IOptimizationPreferences optimizerPreferences,
		    ISchedulePartModifyAndRollbackService rollbackService,
		    IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
    }
}