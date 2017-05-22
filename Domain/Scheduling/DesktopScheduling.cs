using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DesktopScheduling
	{
		private readonly IScheduleExecutor _scheduleExecutor;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceCalculation _resouceResourceOptimizationHelper;

		public DesktopScheduling(IScheduleExecutor scheduleExecutor, 
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IResourceCalculation resouceResourceOptimizationHelper)
		{
			_scheduleExecutor = scheduleExecutor;
			_schedulerStateHolder = schedulerStateHolder;
			_resouceResourceOptimizationHelper = resouceResourceOptimizationHelper;
		}

		public void Execute(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences,
			IDaysOffPreferences dayOffsPreferences)
		{
			_scheduleExecutor.Execute(schedulingCallback, schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod,
				optimizationPreferences, true, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences));

			var resCalcData = _schedulerStateHolder().SchedulingResultState.ToResourceOptimizationData(_schedulerStateHolder().ConsiderShortBreaks, false);
			_resouceResourceOptimizationHelper.ResourceCalculate(selectedPeriod, resCalcData);
		}
	}
}