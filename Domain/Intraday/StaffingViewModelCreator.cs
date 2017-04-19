using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
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
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly ScheduledStaffingToDataSeries _scheduledStaffingToDataSeries;
		private readonly TimeSeriesProvider _timeSeriesProvider;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly ForecastedStaffingToDataSeries _forecastedStaffingToDataSeries;
		private readonly ReforecastedStaffingProvider _reforecastedStaffingProvider;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;

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
			ScheduledStaffingToDataSeries scheduledStaffingToDataSeries,
			TimeSeriesProvider timeSeriesProvider,
			ForecastedStaffingProvider forecastedStaffingProvider,
			ForecastedStaffingToDataSeries forecastedStaffingToDataSeries,
			ReforecastedStaffingProvider reforecastedStaffingProvider,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider
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
			_scheduledStaffingToDataSeries = scheduledStaffingToDataSeries;
			_timeSeriesProvider = timeSeriesProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_forecastedStaffingToDataSeries = forecastedStaffingToDataSeries;
			_reforecastedStaffingProvider = reforecastedStaffingProvider;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
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
			var skillDays = _skillDayRepository.FindReadOnlyRange(userDateOnly.ToDateOnlyPeriod().Inflate(1),skills, scenario);
			
			var actualCallsPerSkillInterval = _intradayQueueStatisticsLoader.LoadActualCallPerSkillInterval(skills, _timeZone.TimeZone(), userDateOnly);
			var latestStatsTime = getLastestStatsTime(actualCallsPerSkillInterval);
			
			var forecastedCallsModel = _forecastedCallsProvider.Load(skills, skillDays, latestStatsTime, minutesPerInterval);
			var scheduledStaffingPerSkill = _scheduledStaffingProvider.StaffingPerSkill(skills, minutesPerInterval, dateOnly, useShrinkage);
			var forecastedStaffing = _forecastedStaffingProvider.StaffingPerSkill(skills, skillDays, minutesPerInterval, dateOnly, useShrinkage);
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
					Date = userDateOnly,
					Time = timeSeries,
					ForecastedStaffing = _forecastedStaffingToDataSeries.DataSeries(forecastedStaffing,timeSeries),
					UpdatedForecastedStaffing = updatedForecastedSeries,
					ActualStaffing = _requiredStaffingProvider.DataSeries(requiredStaffingPerSkill, latestStatsTime, minutesPerInterval, timeSeries),
					ScheduledStaffing = _scheduledStaffingToDataSeries.DataSeries(scheduledStaffingPerSkill, timeSeries)
				},
				StaffingHasData = forecastedStaffing.Any()
			};
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