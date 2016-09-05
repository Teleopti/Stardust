using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedStaffingViewModelCreator
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly IIntradayQueueStatisticsLoader _intradayQueueStatisticsLoader;

		public ForecastedStaffingViewModelCreator(
			INow now,
			IUserTimeZone timeZone,
			ForecastedStaffingProvider forecastedStaffingProvider,
			IIntradayQueueStatisticsLoader intradayQueueStatisticsLoader)
		{
			_now = now;
			_timeZone = timeZone;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_intradayQueueStatisticsLoader = intradayQueueStatisticsLoader;
		}

		public IntradayStaffingViewModel Load(Guid[] skillIdList)
		{
			var now = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersToday = new DateOnly(now);

			var forecastedStaffingModel = _forecastedStaffingProvider.Load(skillIdList);

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

			var actualWorkloadInSeconds = _intradayQueueStatisticsLoader.LoadActualWorkloadInSeconds(skillIdList, _timeZone.TimeZone(), usersToday);

			var workloadDeviationFactor = actualWorkloadInSeconds / forecastedStaffingModel.WorkloadSeconds;

			List<double?> updatedForecastedSeries = staffingForUsersToday
				.Where(s => s.StartTime >= now)
				.Select(t => ((double?)t.Agents * workloadDeviationFactor))
				.ToList();

			var nullCount = staffingForUsersToday.Count() - updatedForecastedSeries.Count;
			for (int i = 0; i < nullCount; i++)
			{
				updatedForecastedSeries.Insert(0, null);
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