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
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IScheduleOvertimeService _scheduleOvertimeService;
		private readonly ScheduleOvertimeOnNonScheduleDays _scheduleOvertimeOnNonScheduleDays;
		private readonly FullResourceCalculation _fullResourceCalculation;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IUserTimeZone _userTimeZone;

		public ScheduleOvertime(Func<ISchedulingResultStateHolder> schedulingResultStateHolder, 
																	IScheduleOvertimeService scheduleOvertimeService,
																	ScheduleOvertimeOnNonScheduleDays scheduleOvertimeOnNonScheduleDays,
																	FullResourceCalculation fullResourceCalculation,
																	IResourceCalculation resourceOptimizationHelper,
																	IUserTimeZone userTimeZone)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_scheduleOvertimeService = scheduleOvertimeService;
			_scheduleOvertimeOnNonScheduleDays = scheduleOvertimeOnNonScheduleDays;
			_fullResourceCalculation = fullResourceCalculation;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_userTimeZone = userTimeZone;
		}

		public void Execute(IOvertimePreferences overtimePreferences, 
										ISchedulingProgress backgroundWorker, 
										IList<IScheduleDay> selectedSchedules)
		{
			_fullResourceCalculation.Execute();
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true, _schedulingResultStateHolder(), _userTimeZone);
			var selectedDates = selectedSchedules.Select(x => x.DateOnlyAsPeriod.DateOnly).Distinct();
			var selectedPersons = selectedSchedules.Select(x => x.Person).Distinct().ToList();
			var cancel = false;
			foreach (var dateOnly in selectedDates)
			{
				var persons = selectedPersons.Randomize();
				foreach (var person in persons)
				{
					//person.Id.GetValueOrDefault() == new Guid("826F2A46-93BB-4B04-8D5E-9B5E015B2577")
					
					if (cancel || checkIfCancelPressed(backgroundWorker)) return;

					var scheduleDay = _schedulingResultStateHolder().Schedules[person].ScheduledDay(dateOnly);
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