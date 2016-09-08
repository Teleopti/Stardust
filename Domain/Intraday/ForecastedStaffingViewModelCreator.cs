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

			forecastedStaffingModel.StaffingIntervals = forecastedStaffingModel.StaffingIntervals
				.GroupBy(g => g.StartTime)
				.Select(s => new StaffingIntervalModel
				{
					StartTime = TimeZoneHelper.ConvertFromUtc(s.First().StartTime, _timeZone.TimeZone()),
					Agents = s.Sum(a => a.Agents)
				}).ToList();

			var staffingForUsersToday = forecastedStaffingModel.StaffingIntervals
												.Where(t => t.StartTime >= usersToday.Date && t.StartTime < usersToday.Date.AddDays(1))
												.ToArray();


			List<double?> updatedForecastedSeries = new List<double?>();
			if (latestStatisticsTimeAndWorkload.LatestStatisticsIntervalId.HasValue)
			{
				var workloadDeviationFactor = latestStatisticsTimeAndWorkload.ActualworkloadInSeconds / forecastedStaffingModel.WorkloadSeconds;

				if (latestStatsTime > usersNow) // This case only for dev, test and demo
					usersNow = latestStatsTime.Value.AddMinutes(minutesPerInterval);

				updatedForecastedSeries = staffingForUsersToday
					.Where(s => s.StartTime >= usersNow)
					.Select(t => ((double?)t.Agents * workloadDeviationFactor))
					.ToList();

				var nullCount = staffingForUsersToday.Count() - updatedForecastedSeries.Count;
				for (int i = 0; i < nullCount; i++)
				{
					updatedForecastedSeries.Insert(0, null);
				}
			}

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
	}
}