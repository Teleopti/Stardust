using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
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
		private readonly SchedulingCommandHandler _schedulingCommandHandler;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceCalculation _resouceResourceOptimizationHelper;
		private readonly ScheduleHourlyStaffExecutor _scheduleHourlyStaffExecutor;

		public DesktopScheduling(SchedulingCommandHandler schedulingCommandHandler, 
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
				ExecuteScheduling(schedulingCallback, schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod, optimizationPreferences, dayOffsPreferences);
			}
			else
			{
				_scheduleHourlyStaffExecutor.Execute(schedulingOptions, backgroundWorker, selectedAgents,
					selectedPeriod, optimizationPreferences, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences));
			}

			var resCalcData = _schedulerStateHolder().SchedulingResultState.ToResourceOptimizationData(_schedulerStateHolder().ConsiderShortBreaks, false);
			_resouceResourceOptimizationHelper.ResourceCalculate(selectedPeriod, resCalcData);
		}

		[RemoveMeWithToggle("move up this", Toggles.ResourcePlanner_SchedulingIslands_44757)]
		protected virtual void ExecuteScheduling(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, IDaysOffPreferences dayOffsPreferences)
		{
			var command = new SchedulingCommand {AgentsToSchedule = selectedAgents, Period = selectedPeriod};
			_schedulingCommandHandler.Execute(command, schedulingCallback, schedulingOptions, backgroundWorker, 
				optimizationPreferences, true, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences));
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_SchedulingIslands_44757)]
	public class DesktopSchedulingOLD : DesktopScheduling
	{
		private readonly IScheduleExecutor _scheduleExecutor;

		public DesktopSchedulingOLD(IScheduleExecutor scheduleExecutor, Func<ISchedulerStateHolder> schedulerStateHolder, IResourceCalculation resouceResourceOptimizationHelper, ScheduleHourlyStaffExecutor scheduleHourlyStaffExecutor) : 
			base(null, schedulerStateHolder, resouceResourceOptimizationHelper, scheduleHourlyStaffExecutor)
		{
			_scheduleExecutor = scheduleExecutor;
		}

		protected override void ExecuteScheduling(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, IDaysOffPreferences dayOffsPreferences)
		{
			_scheduleExecutor.Execute(schedulingCallback, schedulingOptions, backgroundWorker, selectedAgents,
				selectedPeriod, optimizationPreferences, true, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences));
		}
	}
}