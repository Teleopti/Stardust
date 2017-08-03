using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ScheduledStaffingViewModelCreator
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly ScheduledStaffingToDataSeries _scheduledStaffingToDataSeries;
		private readonly TimeSeriesProvider _timeSeriesProvider;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly ForecastedStaffingToDataSeries _forecastedStaffingToDataSeries;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;

		public ScheduledStaffingViewModelCreator(
			INow now,
			IUserTimeZone timeZone,
			IIntervalLengthFetcher intervalLengthFetcher,
			IScenarioRepository scenarioRepository,
			ScheduledStaffingProvider scheduledStaffingProvider,
			ScheduledStaffingToDataSeries scheduledStaffingToDataSeries,
			TimeSeriesProvider timeSeriesProvider,
			ForecastedStaffingProvider forecastedStaffingProvider,
			ForecastedStaffingToDataSeries forecastedStaffingToDataSeries,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			ISkillDayLoadHelper skillDayLoadHelper
		)
		{
			_now = now;
			_timeZone = timeZone;
			_intervalLengthFetcher = intervalLengthFetcher;
			_scenarioRepository = scenarioRepository;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_scheduledStaffingToDataSeries = scheduledStaffingToDataSeries;
			_timeSeriesProvider = timeSeriesProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_forecastedStaffingToDataSeries = forecastedStaffingToDataSeries;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_skillDayLoadHelper = skillDayLoadHelper;
		}

		public ScheduledStaffingViewModel Load(Guid[] skillIdList, DateOnly? dateOnly = null, bool useShrinkage = false)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			if (minutesPerInterval <= 0) throw new Exception($"IntervalLength is cannot be {minutesPerInterval}!");

			var userDateOnly = dateOnly ?? new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()));

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			if (!skills.Any())
				return new ScheduledStaffingViewModel();
			var skillDaysBySkills = _skillDayLoadHelper.LoadSchedulerSkillDays(new DateOnlyPeriod(userDateOnly, userDateOnly.AddDays(1)), skills, scenario);

			calculateForecastedAgentsForEmailSkills(dateOnly, useShrinkage, skillDaysBySkills);

			var forecastedStaffing = _forecastedStaffingProvider.StaffingPerSkill(skillDaysBySkills, minutesPerInterval, dateOnly, useShrinkage);
			var scheduledStaffingPerSkill = _scheduledStaffingProvider.StaffingPerSkill(skills, minutesPerInterval, dateOnly, useShrinkage);

			var timeSeries = _timeSeriesProvider.DataSeries(forecastedStaffing, scheduledStaffingPerSkill, minutesPerInterval);

			var dataSeries = new StaffingDataSeries
			{
				Date = userDateOnly,
				Time = timeSeries,
				ForecastedStaffing = _forecastedStaffingToDataSeries.DataSeries(forecastedStaffing, timeSeries),
				ScheduledStaffing = _scheduledStaffingToDataSeries.DataSeries(scheduledStaffingPerSkill, timeSeries)
			};
			calculateRelativeDifference(dataSeries);
			return new ScheduledStaffingViewModel
			{
				DataSeries = dataSeries,
				StaffingHasData = forecastedStaffing.Any()
			};
		}

		private static void calculateRelativeDifference(StaffingDataSeries dataSeries)
		{
			dataSeries.AbsoluteDifference = new double?[dataSeries.ForecastedStaffing.Length];
			for (var index = 0; index < dataSeries.ForecastedStaffing.Length; index++)
			{
				if (!dataSeries.ForecastedStaffing[index].HasValue) continue;

				if (dataSeries.ScheduledStaffing.Length == 0)
				{
					dataSeries.AbsoluteDifference[index] = -dataSeries.ForecastedStaffing[index];
					continue;
				}

				if (dataSeries.ScheduledStaffing[index].HasValue)
				{
					dataSeries.AbsoluteDifference[index] = Math.Round((double)dataSeries.ScheduledStaffing[index], 1) -
														   Math.Round((double)dataSeries.ForecastedStaffing[index], 1);
					dataSeries.AbsoluteDifference[index] = Math.Round((double) dataSeries.AbsoluteDifference[index],1);
				}
					
			}
		}

		private void calculateForecastedAgentsForEmailSkills(DateOnly? dateOnly, bool useShrinkage,
															 IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays)
		{
			var scheduledStaffingPerSkill = new List<SkillStaffingIntervalLightModel>();
			var skillGroupsByResuolution = skillDays.Keys
				.Where(y => y.SkillType.Description.Name == "SkillTypeEmail")
				.GroupBy(x => x.DefaultResolution);
			foreach (var group in skillGroupsByResuolution)
			{
				var emailSkillsForOneResoultion = group.ToList();
				scheduledStaffingPerSkill.AddRange(_scheduledStaffingProvider.StaffingPerSkill(emailSkillsForOneResoultion, group.Key, dateOnly, useShrinkage));

				foreach (var skill in emailSkillsForOneResoultion)
				{
					var skillDaysEmail = skillDays[skill];
					foreach (var skillDay in skillDaysEmail)
					{
						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							var intervalStartLocal = TimeZoneHelper.ConvertFromUtc(skillStaffPeriod.Period.StartDateTime, _timeZone.TimeZone());
							var scheduledStaff =
								scheduledStaffingPerSkill.FirstOrDefault(
									x => x.Id == skill.Id.Value && x.StartDateTime == intervalStartLocal);
							skillStaffPeriod.SetCalculatedResource65(0);
							if (scheduledStaff.StaffingLevel > 0)
								skillStaffPeriod.SetCalculatedResource65(scheduledStaff.StaffingLevel);
						}
					}
				}
			}
		}

		public IEnumerable<ScheduledStaffingViewModel> Load(Guid[] skillIdList, DateOnlyPeriod dateOnlyPeriod, bool useShrinkage = false)
		{
			return dateOnlyPeriod.DayCollection().Select(day => Load(skillIdList, day, useShrinkage)).ToList();
		}
	}

}