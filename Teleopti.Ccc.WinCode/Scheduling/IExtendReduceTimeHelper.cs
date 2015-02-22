﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IExtendReduceTimeHelper
	{
		void RunExtendReduceTimeOptimization(IOptimizationPreferences optimizerPreferences,
			IBackgroundWorkerWrapper backgroundWorker, IList<IScheduleDay> selectedDays,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixOriginalStateContainer> originalStateListForMoveMax);
	}
}