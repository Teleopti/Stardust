using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IOptimizationCommand
	{
		void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences, ISchedulingProgress backgroundWorker,
			ISchedulerStateHolder schedulerStateHolder, IList<IScheduleDay> selectedScheduleDays,
			IGroupPagePerDateHolder groupPagePerDateHolder, IScheduleOptimizerHelper scheduleOptimizerHelper,
			IOptimizationPreferences optimizationPreferences, bool optimizationMethodBackToLegalState,
			IDaysOffPreferences daysOffPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}
}