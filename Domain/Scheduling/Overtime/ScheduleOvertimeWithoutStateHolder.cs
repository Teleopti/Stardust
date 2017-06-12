using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class ScheduleOvertimeWithoutStateHolder
	{
		private readonly IScheduleOvertimeServiceWithoutStateholder _scheduleOvertimeService;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IUserTimeZone _userTimeZone;

		public ScheduleOvertimeWithoutStateHolder(IScheduleOvertimeServiceWithoutStateholder scheduleOvertimeService,
																	IResourceCalculation resourceOptimizationHelper,
																	IUserTimeZone userTimeZone, IResourceCalculation resourceCalculation)
		{ 
			_scheduleOvertimeService = scheduleOvertimeService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_userTimeZone = userTimeZone;
			_resourceCalculation = resourceCalculation;
		}

		public HashSet<IPerson> Execute(IOvertimePreferences overtimePreferences,
										ISchedulingProgress backgroundWorker,
										IList<IScheduleDay> selectedSchedules,
										DateTimePeriod requestedDateTimePeriod,
										ResourceCalculationData resourceCalculationData,
										Func<IDisposable> contextFunc)
		{

			_resourceCalculation.ResourceCalculate(requestedDateTimePeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc), resourceCalculationData, contextFunc);
			var resourceCalculateDelayer = new ResourceCalculateDelayerWithoutStateHolder(_resourceOptimizationHelper, 1, _userTimeZone);
			var selectedDates = selectedSchedules.Select(x => x.DateOnlyAsPeriod.DateOnly).Distinct();
			var selectedPersons = selectedSchedules.Select(x => x.Person).Distinct().ToList();
			var cancel = false;
			var affectedPersons = new HashSet<IPerson>();
			foreach (var dateOnly in selectedDates)
			{
				var persons = selectedPersons.Randomize();
				
				foreach (var person in persons)
				{
					if (cancel || checkIfCancelPressed(backgroundWorker)) return affectedPersons;

					var scheduleDay = selectedSchedules.FirstOrDefault(x => x.Person == person && x.DateOnlyAsPeriod.DateOnly == dateOnly);
					if (scheduleDay == null) continue;
					IScheduleTagSetter scheduleTagSetter = new ScheduleTagSetter(overtimePreferences.ScheduleTag);
				
					var res = _scheduleOvertimeService.SchedulePersonOnDay(scheduleDay, overtimePreferences, resourceCalculateDelayer, dateOnly, scheduleTagSetter, resourceCalculationData, contextFunc, requestedDateTimePeriod);  
					if (res)
						affectedPersons.Add(person);
					var progressResult = onDayScheduled(backgroundWorker, new SchedulingServiceSuccessfulEventArgs(scheduleDay, () => cancel = true));
					if (progressResult.ShouldCancel) return affectedPersons;
				}
			}
			return affectedPersons;
		}

		private static CancelSignal onDayScheduled(ISchedulingProgress backgroundWorker, SchedulingServiceBaseEventArgs args)
		{
			if (backgroundWorker.CancellationPending)
			{
				args.Cancel = true;
			}

			backgroundWorker.ReportProgress(1, args);
			if (args.Cancel) return new CancelSignal { ShouldCancel = true };

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
