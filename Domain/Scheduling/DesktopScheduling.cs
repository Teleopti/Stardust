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
		private readonly ISchedulingCommandHandler _schedulingCommandHandler;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceCalculation _resouceResourceOptimizationHelper;
		private readonly ScheduleHourlyStaffExecutor _scheduleHourlyStaffExecutor;

		public DesktopScheduling(ISchedulingCommandHandler schedulingCommandHandler, 
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IResourceCalculation resouceResourceOptimizationHelper,
			ScheduleHourlyStaffExecutor scheduleHourlyStaffExecutor)
		{
			_schedulingCommandHandler = schedulingCommandHandler;
			_schedulerStateHolder = schedulerStateHolder;
			_resouceResourceOptimizationHelper = resouceResourceOptimizationHelper;
			_scheduleHourlyStaffExecutor = scheduleHourlyStaffExecutor;
		}

		public void Execute(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences,
			IDaysOffPreferences dayOffsPreferences)
		{
			if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff)
			{
				_schedulingCommandHandler.Execute(schedulingCallback, schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod,
					optimizationPreferences, true, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences));
			}
			else
			{
				_scheduleHourlyStaffExecutor.Execute(schedulingOptions, backgroundWorker, selectedAgents,
					selectedPeriod, optimizationPreferences, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences));
			}

			var resCalcData = _schedulerStateHolder().SchedulingResultState.ToResourceOptimizationData(_schedulerStateHolder().ConsiderShortBreaks, false);
			_resouceResourceOptimizationHelper.ResourceCalculate(selectedPeriod, resCalcData);
		}
	}
}