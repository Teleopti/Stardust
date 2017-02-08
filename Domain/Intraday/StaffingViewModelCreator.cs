using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly TimeSeriesProvider _timeSeriesProvider;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly ReforecastedStaffingProvider _reforecastedStaffingProvider;
		private readonly SupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;

		public StaffingViewModelCreator(
			INow now,
			IUserTimeZone timeZone,
			ForecastedCallsProvider forecastedCallsProvider,
			RequiredStaffingProvider requiredStaffingProvider,
			IIntradayQueueStatisticsLoader intradayQueueStatisticsLoader,
			IIntervalLengthFetcher intervalLengthFetcher,
			ISkillDayRepository skillDayRepository,
			IScenarioRepository scenarioRepository,
			ScheduledStaffingProvider scheduledStaffingProvider,
			TimeSeriesProvider timeSeriesProvider,
			ForecastedStaffingProvider forecastedStaffingProvider,
			ReforecastedStaffingProvider reforecastedStaffingProvider,
			SupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider
			)
		{
			_now = now;
			_timeZone = timeZone;
			_forecastedCallsProvider = forecastedCallsProvider;
			_requiredStaffingProvider = requiredStaffingProvider;
			_intradayQueueStatisticsLoader = intradayQueueStatisticsLoader;
			_intervalLengthFetcher = intervalLengthFetcher;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_timeSeriesProvider = timeSeriesProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_reforecastedStaffingProvider = reforecastedStaffingProvider;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
		}

		public IntradayStaffingViewModel Load(Guid[] skillIdList)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			if (!skills.Any())
				return new IntradayStaffingViewModel();
			var skillDays = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(usersToday.AddDays(-1), usersToday.AddDays(1)),skills, scenario);
			
			var actualCallsPerSkillInterval = _intradayQueueStatisticsLoader.LoadActualCallPerSkillInterval(skills, _timeZone.TimeZone(), usersToday);
			var latestStatsTime = getLastestStatsTime(actualCallsPerSkillInterval);
			
			var forecastedCallsModel = _forecastedCallsProvider.Load(skills, skillDays, latestStatsTime, minutesPerInterval);
			var scheduledStaffingPerSkill = _scheduledStaffingProvider.StaffingPerSkill(skills, minutesPerInterval);
			var forecastedStaffing = _forecastedStaffingProvider.StaffingPerSkill(skills, skillDays, minutesPerInterval);
			var timeSeries = _timeSeriesProvider.DataSeries(forecastedStaffing, scheduledStaffingPerSkill, minutesPerInterval);
			var updatedForecastedSeries = _reforecastedStaffingProvider.DataSeries(
				forecastedStaffing,
				actualCallsPerSkillInterval,
				forecastedCallsModel.CallsPerSkill,
				latestStatsTime,
				minutesPerInterval,
				timeSeries
			);
			var requiredStaffingPerSkill = _requiredStaffingProvider.Load(actualCallsPerSkillInterval, 
				skills, 
				skillDays,
				forecastedStaffing, 
				TimeSpan.FromMinutes(minutesPerInterval),
				forecastedCallsModel.SkillDayStatsRange);
									
			return new IntradayStaffingViewModel
			{
				DataSeries = new StaffingDataSeries
				{
					Time = timeSeries,
					ForecastedStaffing = _forecastedStaffingProvider.DataSeries(forecastedStaffing,timeSeries),
					UpdatedForecastedStaffing = updatedForecastedSeries,
					ActualStaffing = _requiredStaffingProvider.DataSeries(requiredStaffingPerSkill, latestStatsTime, minutesPerInterval, timeSeries),
					ScheduledStaffing = _scheduledStaffingProvider.DataSeries(scheduledStaffingPerSkill, timeSeries)
				},
				StaffingHasData = forecastedStaffing.Any()
			};
		}

		private static DateTime? getLastestStatsTime(IList<SkillIntervalStatistics> actualCallsPerSkillInterval)
		{
			return actualCallsPerSkillInterval.Any() ? (DateTime?)actualCallsPerSkillInterval.Max(d => d.StartTime) : null;
		}
	}

	public interface IStaffingViewModelCreator
	{
		IntradayStaffingViewModel Load(Guid[] skillIdList);
	}
}