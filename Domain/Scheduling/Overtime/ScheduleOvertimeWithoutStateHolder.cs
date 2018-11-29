using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class ScheduleOvertimeWithoutStateHolder
	{
		private readonly ScheduleOvertimeServiceWithoutStateholder _scheduleOvertimeService;
		private readonly IResourceCalculation _resourceCalculation;

		public ScheduleOvertimeWithoutStateHolder(ScheduleOvertimeServiceWithoutStateholder scheduleOvertimeService, IResourceCalculation resourceCalculation)
		{ 
			_scheduleOvertimeService = scheduleOvertimeService;
			_resourceCalculation = resourceCalculation;
		}

		public IList<OverTimeModel> Execute(IOvertimePreferences overtimePreferences,
										ISchedulingProgress backgroundWorker,
										IScheduleDictionary scheduleDictionary,
										IEnumerable<IPerson> agents,
										DateOnlyPeriod period,
										DateTimePeriod requestedDateTimePeriod,
										ResourceCalculationData resourceCalculationData,
										Func<IDisposable> contextFunc)
		{

			_resourceCalculation.ResourceCalculate(requestedDateTimePeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc), resourceCalculationData, contextFunc);
			var cancel = false;
			var overtimeModels = new List<OverTimeModel>();
			foreach (var dateOnly in period.DayCollection())
			{
				var persons = agents.Randomize();

				foreach (var person in persons)
				{
					if (cancel || checkIfCancelPressed(backgroundWorker)) return overtimeModels;

					IScheduleTagSetter scheduleTagSetter = new ScheduleTagSetter(overtimePreferences.ScheduleTag);

					var res = _scheduleOvertimeService.SchedulePersonOnDay(scheduleDictionary[person], overtimePreferences, dateOnly, scheduleTagSetter, resourceCalculationData, contextFunc, requestedDateTimePeriod);
					if (res.HasValue)
						overtimeModels.Add(new OverTimeModel
						{
							PersonId = person.Id.GetValueOrDefault(),
							ActivityId = overtimePreferences.SkillActivity.Id.GetValueOrDefault(),
							StartDateTime = res.Value.StartDateTime,
							EndDateTime = res.Value.EndDateTime
						});
					var progressResult = onDayScheduled(backgroundWorker, new SchedulingServiceSuccessfulEventArgs(scheduleDictionary[person].ScheduledDay(dateOnly), () => cancel = true));
					if (progressResult.ShouldCancel) return overtimeModels;
				}
			}
			return overtimeModels;
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
