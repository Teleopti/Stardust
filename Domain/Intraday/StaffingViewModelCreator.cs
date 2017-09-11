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
		private readonly ISkillTypeInfoProvider _skillTypeInfoProvider;

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
			ISkillDayLoadHelper skillDayLoadHelper,
			ISkillTypeInfoProvider skillTypeInfoProvider
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
			_skillTypeInfoProvider = skillTypeInfoProvider;
		}

		public IntradayStaffingViewModel Load(Guid[] skillIdList, int dayOffset)
		{
			return Load(skillIdList, new DateOnly(_now.UtcDateTime().AddDays(dayOffset)));
		}

		public IntradayStaffingViewModel Load(Guid[] skillIdList, DateOnly? dateOnly = null,  bool useShrinkage = false)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			if (minutesPerInterval <= 0) throw new Exception($"IntervalLength is cannot be {minutesPerInterval}!");

			var timeZone = _timeZone.TimeZone();
			var userDateOnly = dateOnly ?? new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timeZone));

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			if (!skills.Any())
				return new IntradayStaffingViewModel();
			var supportReforecastedAgents = skills.All(x => _skillTypeInfoProvider.GetSkillTypeInfo(x).SupportsReforecastedAgents);

			var skillDaysBySkills = _skillDayLoadHelper.LoadSchedulerSkillDays(new DateOnlyPeriod(userDateOnly, userDateOnly.AddDays(1)), skills, scenario);

			calculateForecastedAgentsForEmailSkills(useShrinkage, skillDaysBySkills, timeZone);

			var forecastedStaffing = _forecastedStaffingProvider.StaffingPerSkill(skillDaysBySkills, minutesPerInterval, dateOnly, useShrinkage);

			var statisticsPerWorkloadInterval = _intradayQueueStatisticsLoader.LoadActualCallPerSkillInterval(skills, timeZone, userDateOnly);

			var latestStatsTime = getLastestStatsTime(statisticsPerWorkloadInterval);
			
			var forecastedCallsModel = _forecastedCallsProvider.Load(skillDaysBySkills, latestStatsTime, minutesPerInterval, dateOnly?.Date);

			var scheduledStaffingPerSkill = _scheduledStaffingProvider.StaffingPerSkill(skills.Any(x => x is MultisiteSkill) ? skillDaysBySkills.Keys.ToList() : skills, minutesPerInterval, dateOnly, useShrinkage);

			var timeSeries = _timeSeriesProvider.DataSeries(forecastedStaffing, scheduledStaffingPerSkill, minutesPerInterval);

			var workloadBacklog = _emailBacklogProvider.GetStatisticsBacklogByWorkload(skillDaysBySkills, statisticsPerWorkloadInterval, userDateOnly, minutesPerInterval, forecastedCallsModel.SkillDayStatsRange);

			var updatedForecastedSeries = supportReforecastedAgents
				? _reforecastedStaffingProvider.DataSeries(
					forecastedStaffing,
					statisticsPerWorkloadInterval,
					forecastedCallsModel.CallsPerSkill,
					latestStatsTime,
					minutesPerInterval,
					timeSeries
				)
				: new double?[0];

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
			return new IntradayStaffingViewModel
			{
				DataSeries = dataSeries,
				StaffingHasData = forecastedStaffing.Any()
			};
		}

		private void calculateForecastedAgentsForEmailSkills(bool useShrinkage, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, TimeZoneInfo timeZone)
		{
			var scheduledStaffingPerSkill = new List<SkillStaffingIntervalLightModel>();
			var skillGroupsByResuolution = skillDays.Keys
				.Where(SkillTypesWithBacklog.IsBacklogSkillType)
				.GroupBy(x => x.DefaultResolution);
			foreach (var group in skillGroupsByResuolution)
			{
				var emailSkillsForOneResoultion = group.ToList();

				foreach (var skill in emailSkillsForOneResoultion)
				{
					var skillDaysEmail = skillDays[skill];
					foreach (var skillDay in skillDaysEmail)
					{
						var skillDayDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(skillDay.CurrentDate.Date, timeZone));
						scheduledStaffingPerSkill.AddRange(_scheduledStaffingProvider.StaffingPerSkill(emailSkillsForOneResoultion, group.Key, skillDayDate, useShrinkage));
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