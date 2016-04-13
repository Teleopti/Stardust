using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class MonitorSkillsProvider
	{
		private readonly IIntradayMonitorDataLoader _intradayMonitorDataLoader;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;

		public MonitorSkillsProvider(IIntradayMonitorDataLoader intradayMonitorDataLoader, IIntervalLengthFetcher intervalLengthFetcher)
		{
			_intradayMonitorDataLoader = intradayMonitorDataLoader;
			_intervalLengthFetcher = intervalLengthFetcher;
		}

		public MonitorDataViewModel Load(Guid [] skillIdList)
		{
			var intervals = _intradayMonitorDataLoader.Load(skillIdList, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone, DateOnly.Today);
			var intervalLength = _intervalLengthFetcher.IntervalLength;

			var summary = new MonitorIntradaySummary();
			var timeSeries = new List<string>();
			var forecastedCallsSeries = new List<double>();
			var forecastedAverageHandleTimeSeries = new List<double>();
			var offeredCallsSeries = new List<double?>();
			var averageHandleTimeSeries = new List<double?>();
		    var latestQueueStatsIntervalId = -1;

			foreach (var interval in intervals)
			{
				summary.ForecastedCalls += interval.ForecastedCalls;
				summary.ForecastedHandleTime += interval.ForecastedHandleTime;
				summary.OfferedCalls += interval.OfferedCalls ?? 0;
				summary.HandleTime += interval.HandleTime ?? 0;
				
				timeSeries.Add(DateTime.Today.Date.AddMinutes(interval.IntervalId * intervalLength).ToShortTimeString());
				forecastedCallsSeries.Add(interval.ForecastedCalls);
				forecastedAverageHandleTimeSeries.Add(interval.ForecastedAverageHandleTime);
				offeredCallsSeries.Add(interval.OfferedCalls);
				averageHandleTimeSeries.Add(interval.AverageHandleTime);

			    if (interval.OfferedCalls.HasValue)
			        latestQueueStatsIntervalId = interval.IntervalId;

			}

			summary.ForecastedAverageHandleTime = summary.ForecastedHandleTime / summary.ForecastedCalls;
			summary.AverageHandleTime = summary.HandleTime / summary.OfferedCalls;

		    summary.ForecastedActualCallsDiff = Math.Abs(summary.ForecastedCalls) < 0.0001
		        ? -99
		        : Math.Round(Math.Abs(summary.ForecastedCalls - summary.OfferedCalls)*100/summary.ForecastedCalls, 1);

		    summary.ForecastedActualHandleTimeDiff = Math.Abs(summary.ForecastedHandleTime) < 0.0001
		        ? -99
		        : Math.Round(Math.Abs(summary.ForecastedHandleTime - summary.HandleTime)*100/summary.ForecastedHandleTime, 1);

		    return new MonitorDataViewModel()
			{
				Summary = summary,
                LatestStatsTime = latestQueueStatsIntervalId == - 1 
                ? String.Empty 
                : DateTime.Today.Date.AddMinutes(latestQueueStatsIntervalId * intervalLength + intervalLength).ToShortTimeString(),
                DataSeries = new MonitorIntradayDataSeries
				{
					Time = timeSeries.ToArray(),
					ForecastedCalls = forecastedCallsSeries.ToArray(),
					ForecastedAverageHandleTime = forecastedAverageHandleTimeSeries.ToArray(),
					OfferedCalls = offeredCallsSeries.ToArray(),
					AverageHandleTime = averageHandleTimeSeries.ToArray()
				}
			};
		}
	}
}