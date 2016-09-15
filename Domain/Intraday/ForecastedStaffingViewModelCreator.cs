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
			var latestStatisticsTimeAndWorkload = _intradayQueueStatisticsLoader.LoadActualWorkloadInSeconds(skillIdList, _timeZone.TimeZone(), usersToday);
			DateTime? latestStatsTime = null;

			if (latestStatisticsTimeAndWorkload.LatestStatisticsIntervalId.HasValue)
				latestStatsTime = usersNow.Date.AddMinutes(latestStatisticsTimeAndWorkload.LatestStatisticsIntervalId.Value*minutesPerInterval);

			var forecastedStaffingModel = _forecastedStaffingProvider.Load(skillIdList, latestStatsTime, minutesPerInterval);

			//forecastedStaffingModel.StaffingIntervals = forecastedStaffingModel.StaffingIntervals
			//	.GroupBy(g => g.StartTime)
			//	.Select(s => new StaffingIntervalModel
			//	{
			//		SkillId = s.First().SkillId,
			//		StartTime = TimeZoneHelper.ConvertFromUtc(s.First().StartTime, _timeZone.TimeZone()),
			//		Agents = s.Sum(a => a.Agents)
			//	})
			//	.OrderBy(o => o.StartTime)
			//	.ToList();

			var staffingForUsersToday = forecastedStaffingModel.StaffingIntervals
												.Where(t => t.StartTime >= usersToday.Date && t.StartTime < usersToday.Date.AddDays(1))
												.ToList();

			var updatedForecastedSeries = getUpdatedForecastedStaffing(
				staffingForUsersToday,
				latestStatisticsTimeAndWorkload.ActualWorkloadInSecondsPerSkill, 
				forecastedStaffingModel.WorkloadInSecondsPerSkill,
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
			IList<SkillWorkload> actualworkloadInSeconds, 
			Dictionary<Guid, List<SkillWorkload>> forecastedWorkloadDictionary, 
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

			var actualWorkloadDictionary = actualworkloadInSeconds
				.GroupBy(g => g.SkillId)
				.Select(s => s)
				.ToDictionary(x => x.Key, x => actualworkloadInSeconds.Where(s => s.SkillId == x.Key));

			var updatedForecastedSeries = new List<StaffingIntervalModel>();

			foreach (var skillId in forecastedWorkloadDictionary.Keys)
			{
				double workloadDeviationFactor = 0;
				IEnumerable<SkillWorkload> actualWorkload = actualWorkloadDictionary[skillId];
				foreach (var forecastedWorkloadInterval in forecastedWorkloadDictionary[skillId])
				{
					var actualWorkloadInterval =
						actualWorkload.SingleOrDefault(x => x.StartTime == forecastedWorkloadInterval.StartTime);

					workloadDeviationFactor += actualWorkloadInterval.WorkloadInSeconds/forecastedWorkloadInterval.WorkloadInSeconds;
				}
				var averageDeviation = workloadDeviationFactor/actualWorkload.Count();
				var updatedForecastedSeriesPerSkill = new List<StaffingIntervalModel>();



				updatedForecastedSeriesPerSkill = forecastedStaffingDictionary[skillId]
					.Where(s => s.StartTime >= usersNow)
					.Select(t => new StaffingIntervalModel
					{
						SkillId = skillId ,
						Agents = t.Agents * averageDeviation,
						StartTime = t.StartTime					
					})
					.ToList();

				var nullCount = forecastedStaffingDictionary[skillId].Count() - updatedForecastedSeriesPerSkill.Count;
				for (int i = 0; i < nullCount; i++)
				{
					updatedForecastedSeriesPerSkill.Insert(0, null);
				}

				updatedForecastedSeries.AddRange(updatedForecastedSeriesPerSkill);
			}

			return updatedForecastedSeries
				.OrderBy(g => g.StartTime)
				.GroupBy(h => h.StartTime)
				.Select(s => (double?)s.Sum(a => a.Agents))
				.ToList();

		}
	}
}