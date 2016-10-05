using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DesktopScheduling
	{
		private readonly ScheduleCommand _scheduleCommand;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceOptimization _resouceResourceOptimizationHelper;

		public DesktopScheduling(ScheduleCommand scheduleCommand, 
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IResourceOptimization resouceResourceOptimizationHelper)
		{
			_scheduleCommand = scheduleCommand;
			_schedulerStateHolder = schedulerStateHolder;
			_resouceResourceOptimizationHelper = resouceResourceOptimizationHelper;
		}

		public void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences,
			ISchedulingProgress backgroundWorker, IList<IScheduleDay> selectedScheduleDays,
			IOptimizationPreferences optimizationPreferences,
			IDaysOffPreferences dayOffsPreferences)
		{
			_scheduleCommand.Execute(optimizerOriginalPreferences, backgroundWorker, selectedScheduleDays,
				optimizationPreferences, true,
				new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences));

			//TODO: (probably) enough to shovel resources here (if cascading is turned on) - no need to do res calc
			var resCalcData = _schedulerStateHolder().SchedulingResultState.ToResourceOptimizationData(_schedulerStateHolder().ConsiderShortBreaks, false);
			selectedScheduleDays.Select(x => x.DateOnlyAsPeriod.DateOnly).Distinct()
				.ForEach(x => _resouceResourceOptimizationHelper.ResourceCalculate(x, resCalcData));
		}
	}
}