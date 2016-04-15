using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IDayOffOptimization
	{
		void Execute(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization,
													DateOnlyPeriod selectedPeriod,
													ISchedulingProgress backgroundWorker,
													IOptimizationPreferences optimizerPreferences,
													IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}
}