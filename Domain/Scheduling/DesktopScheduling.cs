using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

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

		public void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences,
			ISchedulingProgress backgroundWorker, IEnumerable<IScheduleDay> selectedScheduleDays,
			IOptimizationPreferences optimizationPreferences,
			IDaysOffPreferences dayOffsPreferences)
		{
			_scheduleExecutor.Execute(optimizerOriginalPreferences, backgroundWorker, selectedScheduleDays,
				optimizationPreferences, true,
				new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences));

			//TODO: (probably) enough to shovel resources here (if cascading is turned on) - no need to do res calc
			var resCalcData = _schedulerStateHolder().SchedulingResultState.ToResourceOptimizationData(_schedulerStateHolder().ConsiderShortBreaks, false);
			selectedScheduleDays.Select(x => x.DateOnlyAsPeriod.DateOnly).Distinct()
				.ForEach(x => _resouceResourceOptimizationHelper.ResourceCalculate(x, resCalcData));
		}
	}
}