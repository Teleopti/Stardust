using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleOvertime
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleOvertimeService _scheduleOvertimeService;
		private readonly ScheduleOvertimeOnNonScheduleDays _scheduleOvertimeOnNonScheduleDays;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IUserTimeZone _userTimeZone;

		public ScheduleOvertime(Func<ISchedulerStateHolder> schedulerStateHolder, 
																	IScheduleOvertimeService scheduleOvertimeService,
																	ScheduleOvertimeOnNonScheduleDays scheduleOvertimeOnNonScheduleDays,
																	IResourceCalculation resourceOptimizationHelper,
																	IUserTimeZone userTimeZone, IResourceCalculation resourceCalculation)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleOvertimeService = scheduleOvertimeService;
			_scheduleOvertimeOnNonScheduleDays = scheduleOvertimeOnNonScheduleDays;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_userTimeZone = userTimeZone;
			_resourceCalculation = resourceCalculation;
		}

		public void Execute(IOvertimePreferences overtimePreferences, 
										ISchedulingProgress backgroundWorker, 
										IList<IScheduleDay> selectedSchedules)
		{
			var stateholder = _schedulerStateHolder();
			_resourceCalculation.ResourceCalculate(stateholder.RequestedPeriod.DateOnlyPeriod, stateholder.SchedulingResultState.ToResourceOptimizationData(stateholder.ConsiderShortBreaks, false));
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true, stateholder.SchedulingResultState, _userTimeZone);
			var selectedDates = selectedSchedules.Select(x => x.DateOnlyAsPeriod.DateOnly).Distinct();
			var selectedPersons = selectedSchedules.Select(x => x.Person).Distinct().ToList();
			var cancel = false;
			foreach (var dateOnly in selectedDates)
			{
				var persons = selectedPersons.Randomize();
				foreach (var person in persons)
				{
					if (cancel || checkIfCancelPressed(backgroundWorker)) return;

					var scheduleDay = _schedulerStateHolder().Schedules[person].ScheduledDay(dateOnly);
					IScheduleTagSetter scheduleTagSetter = new ScheduleTagSetter(overtimePreferences.ScheduleTag);
					_scheduleOvertimeService.SchedulePersonOnDay(scheduleDay, overtimePreferences, resourceCalculateDelayer, dateOnly, scheduleTagSetter);
					_scheduleOvertimeOnNonScheduleDays.SchedulePersonOnDay(scheduleDay, overtimePreferences, resourceCalculateDelayer);
					var progressResult = onDayScheduled(backgroundWorker,new SchedulingServiceSuccessfulEventArgs(scheduleDay,()=>cancel=true));
					if (progressResult.ShouldCancel) return;
				}
			}
		}

		private static CancelSignal onDayScheduled(ISchedulingProgress backgroundWorker, SchedulingServiceBaseEventArgs args)
		{
			if (backgroundWorker.CancellationPending)
			{
				args.Cancel = true;
			}

			backgroundWorker.ReportProgress(1, args);
			if (args.Cancel) return new CancelSignal{ShouldCancel = true};

			return new CancelSignal();
		}

		private static bool checkIfCancelPressed(ISchedulingProgress backgroundWorker)
		{
			if (backgroundWorker.CancellationPending)
				return true;
			return false;
		}
	}
}