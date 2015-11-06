using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeScheduleCommand : IScheduleCommand
	{
		public void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences, IBackgroundWorkerWrapper backgroundWorker,
			ISchedulerStateHolder schedulerStateHolder, IList<IScheduleDay> selectedScheduleDays, IGroupPagePerDateHolder groupPagePerDateHolder,
			IRequiredScheduleHelper requiredScheduleOptimizerHelper, 
			IOptimizationPreferences optimizationPreferences, bool runWeeklyRestSolver, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			Executed = true;
		}

		public bool Executed { get; private set; }
	}
}