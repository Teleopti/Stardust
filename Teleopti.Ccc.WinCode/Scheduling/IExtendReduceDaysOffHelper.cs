using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IExtendReduceDaysOffHelper
	{
		void RunExtendReduceDayOffOptimization(IOptimizationPreferences optimizerPreferences,
			IBackgroundWorkerWrapper backgroundWorker, IList<IScheduleDay> selectedDays,
			ISchedulerStateHolder schedulerStateHolder,
			DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixOriginalStateContainer> originalStateListForMoveMax);
	}
}