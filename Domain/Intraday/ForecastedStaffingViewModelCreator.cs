using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedStaffingViewModelCreator
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly ForecastedCallsProvider _forecastedCallsProvider;
		private readonly RequiredStaffingProvider _requiredStaffingProvider;
		private readonly IIntradayQueueStatisticsLoader _intradayQueueStatisticsLoader;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly SupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly TimeSeriesProvider _timeSeriesProvider;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;

		public ForecastedStaffingViewModelCreator(
			INow now,
			IUserTimeZone timeZone,
			ForecastedCallsProvider forecastedCallsProvider,
			RequiredStaffingProvider requiredStaffingProvider,
			IIntradayQueueStatisticsLoader intradayQueueStatisticsLoader,
			IIntervalLengthFetcher intervalLengthFetcher,
			ISkillRepository skillRepository,
			ISkillDayRepository skillDayRepository,
			IScenarioRepository scenarioRepository,
			ScheduledStaffingProvider scheduledStaffingProvider,
			TimeSeriesProvider timeSeriesProvider,
			ForecastedStaffingProvider forecastedStaffingProvider
			)
		{
			_now = now;
			_timeZone = timeZone;
			_forecastedCallsProvider = forecastedCallsProvider;
			_requiredStaffingProvider = requiredStaffingProvider;
			_intradayQueueStatisticsLoader = intradayQueueStatisticsLoader;
			_intervalLengthFetcher = intervalLengthFetcher;
			_skillRepository = skillRepository;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_timeSeriesProvider = timeSeriesProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_supportedSkillsInIntradayProvider = new SupportedSkillsInIntradayProvider(skillRepository);
		}

		public IntradayStaffingViewModel Load(Guid[] skillIdList)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			var skillDays = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(usersToday.AddDays(-1), usersToday.AddDays(1)),skills, scenario);
			
			var actualCallsPerSkillInterval = _intradayQueueStatisticsLoader.LoadActualCallPerSkillInterval(skills, _timeZone.TimeZone(), usersToday);
			DateTime? latestStatsTime = null;

			if (actualCallsPerSkillInterval.Count > 0)
				latestStatsTime = actualCallsPerSkillInterval.Max(d => d.StartTime);

			var forecastedCallsModel = _forecastedCallsProvider.Load(skills, skillDays, latestStatsTime, minutesPerInterval);

			var scheduledStaffing = _scheduledStaffingProvider.StaffingPerSkill(skills, minutesPerInterval);

			var forecastedStaffing = _forecastedStaffingProvider.StaffingPerSkill(skills, skillDays, minutesPerInterval);

			var timeSeries = _timeSeriesProvider.DataSeries(forecastedStaffing, scheduledStaffing, minutesPerInterval);

			var updatedForecastedSeries = getUpdatedForecastedStaffing(
				forecastedStaffing,
				actualCallsPerSkillInterval,
				forecastedCallsModel.CallsPerSkill,
				latestStatsTime,
				usersNow,
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
					UpdatedForecastedStaffing = updatedForecastedSeries.ToArray(),
					ActualStaffing = _requiredStaffingProvider.DataSeries(requiredStaffingPerSkill, latestStatsTime, minutesPerInterval, timeSeries),
					ScheduledStaffing = _scheduledStaffingProvider.DataSeries(scheduledStaffing, timeSeries)
				}
			};
		}


		private List<double?> getUpdatedForecastedStaffing(IList<StaffingIntervalModel> forecastedStaffingList, 
			IList<SkillIntervalStatistics> actualCallsPerSkillList, 
			Dictionary<Guid, List<SkillIntervalStatistics>> forecastedCallsPerSkillDictionary, 
			DateTime? latestStatsTime, 
			DateTime usersNow, 
			int minutesPerInterval, 
			DateTime[] timeSeries)
		{
			if (!latestStatsTime.HasValue)
				return new List<double?>();

			if (forecastedCallsPerSkillDictionary.Count(x => x.Value.Any()) == 0)
			{
				return new List<double?>();
			}

			if (latestStatsTime > usersNow) // This case only for dev, test and demo
				usersNow = latestStatsTime.Value.AddMinutes(minutesPerInterval);

			var updatedForecastedSeries = new List<StaffingIntervalModel>();

			foreach (var skillId in forecastedCallsPerSkillDictionary.Keys)
			{
				List<SkillIntervalStatistics> actualStats = actualCallsPerSkillList.Where(x => x.SkillId == skillId).ToList();
				List<double> listDeviationFactorPerInterval = new List<double>();
				double averageDeviation = 1;
				if (actualStats.Count > 0)
				{
					int intervalCounter = 0;
					foreach (var forecastedIntervalCalls in forecastedCallsPerSkillDictionary[skillId])
					{
						var actualIntervalCalls =
							actualStats.SingleOrDefault(x => x.StartTime == forecastedIntervalCalls.StartTime);
						if (actualIntervalCalls == null)
							continue;
						if (Math.Abs(forecastedIntervalCalls.Calls) < 0.1)
							continue;

						intervalCounter++;
						listDeviationFactorPerInterval.Add(actualIntervalCalls.Calls / forecastedIntervalCalls.Calls);
					}
					var alpha = 0.2d;
					if (listDeviationFactorPerInterval.Count != 0)
						averageDeviation = listDeviationFactorPerInterval.Aggregate((current, next) => alpha*next + (1 - alpha)*current);
				}


				var updatedForecastedSeriesPerSkill = forecastedStaffingList
					.Where(x => x.SkillId == skillId && x.StartTime >= usersNow)
					.Select(t => new StaffingIntervalModel
					{
						SkillId = skillId,
						Agents = t.Agents * averageDeviation,
						StartTime = t.StartTime
					})
					.OrderBy(y => y.StartTime)
					.ToList();

				updatedForecastedSeries.AddRange(updatedForecastedSeriesPerSkill);
			}

			if (!updatedForecastedSeries.Any())
			{
				return new List<double?>();
			}

			var returnValue = updatedForecastedSeries
				.OrderBy(g => g.StartTime)
				.GroupBy(h => h.StartTime)
				.Select(s => (double?)s.Sum(a => a.Agents))
				.ToList();


			var timeSeriesStart = timeSeries.Min();
			var reforecastStart = updatedForecastedSeries
				.Min(m => m.StartTime);

			for (DateTime i = timeSeriesStart; i < reforecastStart; i = i.AddMinutes(minutesPerInterval))
			{
				returnValue.Insert(0, null);
			}

			var reforecastEnd = updatedForecastedSeries
				.Max(m => m.StartTime);
			var timeSeriesEnd = timeSeries.Max().AddMinutes(minutesPerInterval);

			for (DateTime j = reforecastEnd.AddMinutes(minutesPerInterval); j < timeSeriesEnd; j = j.AddMinutes(minutesPerInterval))
			{
				returnValue.Add(null);
			}

			return returnValue;
		}
	}
}