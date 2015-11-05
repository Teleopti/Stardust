using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IExtendReduceDaysOffHelper
	{
		void RunExtendReduceDayOffOptimization(IOptimizationPreferences optimizerPreferences,
			IBackgroundWorkerWrapper backgroundWorker, IList<IScheduleDay> selectedDays,
			ISchedulerStateHolder schedulerStateHolder,
			DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixOriginalStateContainer> originalStateListForMoveMax,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}
}