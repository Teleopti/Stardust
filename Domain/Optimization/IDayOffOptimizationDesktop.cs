using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	//remove me when toggle 37998 is set to true
	public interface IDayOffOptimizationDesktop
	{
		void Execute(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization,
													DateOnlyPeriod selectedPeriod,
													ISchedulingProgress backgroundWorker,
													IOptimizationPreferences optimizationPreferences,
													IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}
}