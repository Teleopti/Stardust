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
	public class StaffingViewModelCreator : IStaffingViewModelCreator
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly ForecastedCallsProvider _forecastedCallsProvider;
		private readonly RequiredStaffingProvider _requiredStaffingProvider;
		private readonly IIntradayQueueStatisticsLoader _intradayQueueStatisticsLoader;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly ScheduledStaffingToDataSeries _scheduledStaffingToDataSeries;
		private readonly TimeSeriesProvider _timeSeriesProvider;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly ForecastedStaffingToDataSeries _forecastedStaffingToDataSeries;
		private readonly ReforecastedStaffingProvider _reforecastedStaffingProvider;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly EmailBacklogProvider _emailBacklogProvider;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;

		public StaffingViewModelCreator(
			INow now,
			IUserTimeZone timeZone,
			ForecastedCallsProvider forecastedCallsProvider,
			RequiredStaffingProvider requiredStaffingProvider,
			IIntradayQueueStatisticsLoader intradayQueueStatisticsLoader,
			IIntervalLengthFetcher intervalLengthFetcher,
			IScenarioRepository scenarioRepository,
			ScheduledStaffingProvider scheduledStaffingProvider,
			ScheduledStaffingToDataSeries scheduledStaffingToDataSeries,
			TimeSeriesProvider timeSeriesProvider,
			ForecastedStaffingProvider forecastedStaffingProvider,
			ForecastedStaffingToDataSeries forecastedStaffingToDataSeries,
			ReforecastedStaffingProvider reforecastedStaffingProvider,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			EmailBacklogProvider emailBacklogProvider,
			ISkillDayLoadHelper skillDayLoadHelper
			)
		{
			_now = now;
			_timeZone = timeZone;
			_forecastedCallsProvider = forecastedCallsProvider;
			_requiredStaffingProvider = requiredStaffingProvider;
			_intradayQueueStatisticsLoader = intradayQueueStatisticsLoader;
			_intervalLengthFetcher = intervalLengthFetcher;
			_scenarioRepository = scenarioRepository;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_scheduledStaffingToDataSeries = scheduledStaffingToDataSeries;
			_timeSeriesProvider = timeSeriesProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_forecastedStaffingToDataSeries = forecastedStaffingToDataSeries;
			_reforecastedStaffingProvider = reforecastedStaffingProvider;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_emailBacklogProvider = emailBacklogProvider;
			_skillDayLoadHelper = skillDayLoadHelper;
		}

		public IntradayStaffingViewModel Load(Guid[] skillIdList, DateOnly? dateOnly = null,  bool useShrinkage = false)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			if (minutesPerInterval <= 0) throw new Exception($"IntervalLength is cannot be {minutesPerInterval}!");

			var userDateOnly = dateOnly ?? new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()));
			

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			if (!skills.Any())
				return new IntradayStaffingViewModel();
			var skillDaysBySkills = _skillDayLoadHelper.LoadSchedulerSkillDays(new DateOnlyPeriod(userDateOnly, userDateOnly.AddDays(1)),skills, scenario);

			calculateForecastedAgentsForEmailSkills(dateOnly, useShrinkage, skillDaysBySkills);

			var forecastedStaffing = _forecastedStaffingProvider.StaffingPerSkill(skillDaysBySkills, minutesPerInterval, dateOnly, useShrinkage);

			var statisticsPerWorkloadInterval = _intradayQueueStatisticsLoader.LoadActualCallPerSkillInterval(skills, _timeZone.TimeZone(), userDateOnly);

			var latestStatsTime = getLastestStatsTime(statisticsPerWorkloadInterval);
			
			var forecastedCallsModel = _forecastedCallsProvider.Load(skillDaysBySkills, latestStatsTime, minutesPerInterval);
			var scheduledStaffingPerSkill = _scheduledStaffingProvider.StaffingPerSkill(skills, minutesPerInterval, dateOnly, useShrinkage);
			
			var timeSeries = _timeSeriesProvider.DataSeries(forecastedStaffing, scheduledStaffingPerSkill, minutesPerInterval);

			var workloadBacklog = _emailBacklogProvider.GetStatisticsBacklogByWorkload(skillDaysBySkills, statisticsPerWorkloadInterval, userDateOnly, minutesPerInterval, forecastedCallsModel.SkillDayStatsRange);

			var updatedForecastedSeries = _reforecastedStaffingProvider.DataSeries(
				forecastedStaffing,
				statisticsPerWorkloadInterval,
				forecastedCallsModel.CallsPerSkill,
				latestStatsTime,
				minutesPerInterval,
				timeSeries
			);
			var requiredStaffingPerSkill = _requiredStaffingProvider.Load(statisticsPerWorkloadInterval, 
				skills,
				skillDaysBySkills,
				forecastedStaffing, 
				TimeSpan.FromMinutes(minutesPerInterval),
				forecastedCallsModel.SkillDayStatsRange,
				workloadBacklog
			);

			var dataSeries = new StaffingDataSeries
			{
				Date = userDateOnly,
				Time = timeSeries,
				ForecastedStaffing = _forecastedStaffingToDataSeries.DataSeries(forecastedStaffing, timeSeries),
				UpdatedForecastedStaffing = updatedForecastedSeries,
				ActualStaffing = _requiredStaffingProvider.DataSeries(requiredStaffingPerSkill, latestStatsTime, minutesPerInterval, timeSeries),
				ScheduledStaffing = _scheduledStaffingToDataSeries.DataSeries(scheduledStaffingPerSkill, timeSeries)
			};
			calculateRelativeDifference(dataSeries);
			return new IntradayStaffingViewModel
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
				if (dataSeries.ForecastedStaffing[index].HasValue)
				{
					if ( dataSeries.ScheduledStaffing.Length == 0)
					{
						dataSeries.AbsoluteDifference[index] = -dataSeries.ForecastedStaffing[index];
						continue;
					}

					if (dataSeries.ScheduledStaffing[index].HasValue)
						dataSeries.AbsoluteDifference[index] = dataSeries.ScheduledStaffing[index] - dataSeries.ForecastedStaffing[index];
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

		public IEnumerable<IntradayStaffingViewModel> Load(Guid[] skillIdList, DateOnlyPeriod dateOnlyPeriod, bool useShrinkage = false)
		{
			return dateOnlyPeriod.DayCollection().Select(day => Load(skillIdList, day, useShrinkage)).ToList();
		}

		private static DateTime? getLastestStatsTime(IList<SkillIntervalStatistics> actualCallsPerSkillInterval)
		{
			return actualCallsPerSkillInterval.Any() ? (DateTime?)actualCallsPerSkillInterval.Max(d => d.StartTime) : null;
		}
	}

}