using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Staffing;
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
			var resourceCalculateDelayer = new ResourceCalculateDelayerWithoutStateHolder(_resourceOptimizationHelper, 1, _userTimeZone);
			var cancel = false;
			var overtimeModels = new List<OverTimeModel>();
			foreach (var dateOnly in period.DayCollection())
			{
				var persons = agents.Randomize();

				foreach (var person in persons)
				{
					if (cancel || checkIfCancelPressed(backgroundWorker)) return overtimeModels;

					IScheduleTagSetter scheduleTagSetter = new ScheduleTagSetter(overtimePreferences.ScheduleTag);

					var res = _scheduleOvertimeService.SchedulePersonOnDay(scheduleDictionary[person], overtimePreferences, resourceCalculateDelayer, dateOnly, scheduleTagSetter, resourceCalculationData, contextFunc, requestedDateTimePeriod);
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
