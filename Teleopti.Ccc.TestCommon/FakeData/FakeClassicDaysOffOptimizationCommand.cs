using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeClassicDaysOffOptimizationCommand : IClassicDaysOffOptimizationCommand
	{
		public void Execute(IList<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod, IOptimizationPreferences optimizationPreferences,
			ISchedulerStateHolder schedulerStateHolder, IBackgroundWorkerWrapper backgroundWorker, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			OptimizationExecute = true;
		}

		public bool OptimizationExecute = false;
	}
}