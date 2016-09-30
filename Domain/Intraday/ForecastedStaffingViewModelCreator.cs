using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedStaffingViewModelCreator
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly IIntradayQueueStatisticsLoader _intradayQueueStatisticsLoader;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;

		public ForecastedStaffingViewModelCreator(
			INow now,
			IUserTimeZone timeZone,
			ForecastedStaffingProvider forecastedStaffingProvider,
			IIntradayQueueStatisticsLoader intradayQueueStatisticsLoader,
			IIntervalLengthFetcher intervalLengthFetcher)
		{
			_now = now;
			_timeZone = timeZone;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_intradayQueueStatisticsLoader = intradayQueueStatisticsLoader;
			_intervalLengthFetcher = intervalLengthFetcher;
		}

		public IntradayStaffingViewModel Load(Guid[] skillIdList)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);
			var actualCallsPerSkillInterval = _intradayQueueStatisticsLoader.LoadActualCallPerSkillInterval(skillIdList, _timeZone.TimeZone(), usersToday);
			DateTime? latestStatsTime = null;

			if (actualCallsPerSkillInterval.Count > 0)
				latestStatsTime = actualCallsPerSkillInterval.Max(d => d.StartTime);

			var forecastedStaffingModel = _forecastedStaffingProvider.Load(skillIdList, latestStatsTime, minutesPerInterval);

			var staffingForUsersToday = forecastedStaffingModel.StaffingIntervals
												.Where(t => t.StartTime >= usersToday.Date && t.StartTime < usersToday.Date.AddDays(1))
												.ToList();

			var updatedForecastedSeries = getUpdatedForecastedStaffing(
				staffingForUsersToday,
				actualCallsPerSkillInterval, 
				forecastedStaffingModel.CallsPerSkill,
				latestStatsTime, 
				usersNow, 
				minutesPerInterval
			);

			staffingForUsersToday = staffingForUsersToday
				.GroupBy(g => g.StartTime)
				.Select(s => new StaffingIntervalModel
				{
					SkillId = s.First().SkillId,
					StartTime = s.First().StartTime,
					Agents = s.Sum(a => a.Agents)
				})
				.OrderBy(o => o.StartTime)
				.ToList();

			return new IntradayStaffingViewModel()
			{
				DataSeries = new StaffingDataSeries()
				{
					Time = staffingForUsersToday
								.Select(t => t.StartTime)
								.ToArray(),
					ForecastedStaffing = staffingForUsersToday
								.Select(t => t.Agents)
								.ToArray(),
					UpdatedForecastedStaffing = updatedForecastedSeries
								.ToArray()
				}
			};
		}

		private List<double?> getUpdatedForecastedStaffing(
			List<StaffingIntervalModel> forecastedStaffingList, 
			IList<SkillIntervalCalls> actualCallsPerSkillList, 
			Dictionary<Guid, List<SkillIntervalCalls>> forecastedCallsPerSkillDictionary, 
			DateTime? latestStatsTime, 
			DateTime usersNow, 
			int minutesPerInterval)
		{


			if (!latestStatsTime.HasValue)
				return new List<double?>();

			if (latestStatsTime > usersNow) // This case only for dev, test and demo
				usersNow = latestStatsTime.Value.AddMinutes(minutesPerInterval);

			var forecastedStaffingDictionary = forecastedStaffingList
				.GroupBy(g => g.SkillId)
				.Select(s => s)
				.ToDictionary(x => x.Key, x => forecastedStaffingList.Where(s => s.SkillId == x.Key));

			var actualCallsPerSkillDictionary = actualCallsPerSkillList
				.GroupBy(g => g.SkillId)
				.Select(s => s)
				.ToDictionary(x => x.Key, x => actualCallsPerSkillList.Where(s => s.SkillId == x.Key));

			var updatedForecastedSeries = new List<StaffingIntervalModel>();

			foreach (var skillId in forecastedCallsPerSkillDictionary.Keys)
			{
				double averageDeviation = 1;
				if (actualCallsPerSkillDictionary.ContainsKey(skillId))
				{
					IEnumerable<SkillIntervalCalls> actualCalls = actualCallsPerSkillDictionary[skillId];
					double callsDeviationFactor = 0;
					foreach (var forecastedIntervalCalls in forecastedCallsPerSkillDictionary[skillId])
					{
						var actualIntervalCalls =
							actualCalls.SingleOrDefault(x => x.StartTime == forecastedIntervalCalls.StartTime);
						if (actualIntervalCalls == null)
							continue;
						callsDeviationFactor += actualIntervalCalls.Calls / forecastedIntervalCalls.Calls;
					}

					averageDeviation = callsDeviationFactor / actualCalls.Count();
				}
				
				var updatedForecastedSeriesPerSkill = forecastedStaffingDictionary[skillId]
					.Where(s => s.StartTime >= usersNow)
					.Select(t => new StaffingIntervalModel
					{
						SkillId = skillId ,
						Agents = t.Agents * averageDeviation,
						StartTime = t.StartTime					
					})
					.ToList();

				updatedForecastedSeries.AddRange(updatedForecastedSeriesPerSkill);
			}

			var returnValue = updatedForecastedSeries
				.OrderBy(g => g.StartTime)
				.GroupBy(h => h.StartTime)
				.Select(s => (double?)s.Sum(a => a.Agents))
				.ToList();

			var nullStartTime = forecastedStaffingDictionary
				.Select(x => x.Value.Min(m => m.StartTime))
				.Min(t => t);
			var nullEndTime = updatedForecastedSeries.Min(m => m.StartTime);
			for (DateTime i = nullStartTime; i < nullEndTime; i = i.AddMinutes(minutesPerInterval))
			{
				returnValue.Insert(0, null);
			}

			return returnValue;

		}
	}
}