using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IScheduleOptimizerHelper
	{
		void ResetWorkShiftFinderResults();

		void GetBackToLegalState(IList<IScheduleMatrixPro> matrixList,
			ISchedulerStateHolder schedulerStateHolder,
			ISchedulingProgress backgroundWorker,
			ISchedulingOptions schedulingOptions,
			DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixPro> allMatrixes);

		void DaysOffBackToLegalState(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
			ISchedulingProgress backgroundWorker,
			IDayOffTemplate dayOffTemplate,
			bool reschedule,
			ISchedulingOptions schedulingOptions,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);

		void ReOptimize(ISchedulingProgress backgroundWorker, IList<IScheduleDay> selectedDays, ISchedulingOptions schedulingOptions, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}
}