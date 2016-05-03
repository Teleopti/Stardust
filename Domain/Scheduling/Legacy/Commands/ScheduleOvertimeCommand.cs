using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleOvertimeCommand : IScheduleOvertimeCommand
	{
		private readonly Func<ISchedulerStateHolder> _schedulerState;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IScheduleOvertimeService _scheduleOvertimeService;
		private readonly ScheduleOvertimeOnNonScheduleDays _scheduleOvertimeOnNonScheduleDays;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;

		public ScheduleOvertimeCommand(Func<ISchedulerStateHolder> schedulerState, 
																	Func<ISchedulingResultStateHolder> schedulingResultStateHolder, 
																	IScheduleOvertimeService scheduleOvertimeService,
																	ScheduleOvertimeOnNonScheduleDays scheduleOvertimeOnNonScheduleDays,
																	IResourceOptimizationHelper resourceOptimizationHelper,
																	Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended)
		{
			_schedulerState = schedulerState;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_scheduleOvertimeService = scheduleOvertimeService;
			_scheduleOvertimeOnNonScheduleDays = scheduleOvertimeOnNonScheduleDays;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
		}

		public void Execute(IOvertimePreferences overtimePreferences, ISchedulingProgress backgroundWorker, IList<IScheduleDay> selectedSchedules, IGridlockManager gridlockManager)
		{
			//currently needed for _scheduleOvertimeOnNonScheduleDays to work - if too slow, refactor!
			_resourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);
			//
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true);
			var selectedDates = selectedSchedules.Select(x => x.DateOnlyAsPeriod.DateOnly).Distinct();
			var selectedPersons = selectedSchedules.Select(x => x.Person).Distinct().ToList();
			var cancel = false;
			foreach (var dateOnly in selectedDates)
			{
				var persons = selectedPersons.Randomize();
				foreach (var person in persons)
				{
					if (cancel || checkIfCancelPressed(backgroundWorker)) return;
					
					var locks = gridlockManager.Gridlocks(person, dateOnly);
					if (locks != null && locks.Count != 0) continue;

					var scheduleDay = _schedulingResultStateHolder().Schedules[person].ScheduledDay(dateOnly);
					var rules = NewBusinessRuleCollection.Minimum();
					IScheduleTagSetter scheduleTagSetter = new ScheduleTagSetter(overtimePreferences.ScheduleTag);
					_scheduleOvertimeService.SchedulePersonOnDay(scheduleDay, overtimePreferences, resourceCalculateDelayer, dateOnly, rules, scheduleTagSetter, _schedulerState().TimeZoneInfo);
					_scheduleOvertimeOnNonScheduleDays.SchedulePersonOnDay(scheduleDay, overtimePreferences, resourceCalculateDelayer);
					var progressResult = onDayScheduled(backgroundWorker,new SchedulingServiceSuccessfulEventArgs(scheduleDay,()=>cancel=true));
					if (progressResult.ShouldCancel) return;
				}
			}
		}

		private CancelSignal onDayScheduled(ISchedulingProgress backgroundWorker, SchedulingServiceBaseEventArgs args)
		{
			if (backgroundWorker.CancellationPending)
			{
				args.Cancel = true;
			}

			backgroundWorker.ReportProgress(1, args);
			if (args.Cancel) return new CancelSignal{ShouldCancel = true};

			return new CancelSignal();
		}

		private bool checkIfCancelPressed(ISchedulingProgress backgroundWorker)
		{
			if (backgroundWorker.CancellationPending)
				return true;
			return false;
		}
	}
}