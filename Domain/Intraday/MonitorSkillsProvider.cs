using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
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
		private readonly INow _now;

		public MonitorSkillsProvider(IIntradayMonitorDataLoader intradayMonitorDataLoader, IIntervalLengthFetcher intervalLengthFetcher, INow now)
		{
			_intradayMonitorDataLoader = intradayMonitorDataLoader;
			_intervalLengthFetcher = intervalLengthFetcher;
			_now = now;
		}

		public MonitorDataViewModel Load(Guid[] skillIdList)
		{
			var intervals = _intradayMonitorDataLoader.Load(skillIdList, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone, _now.LocalDateOnly());
			var intervalLength = _intervalLengthFetcher.IntervalLength;

			var summary = new MonitorIntradaySummary();
			var timeSeries = new List<DateTime>();
			var forecastedCallsSeries = new List<double>();
			var forecastedAverageHandleTimeSeries = new List<double>();
			var offeredCallsSeries = new List<double?>();
			var averageHandleTimeSeries = new List<double?>();
			var latestQueueStatsIntervalId = -1;
			var averageSpeedOfAnswer = new List<double?>();
			var abandonedRate = new List<double?>();
			var serviceLevel = new List<double?>();

			foreach (var interval in intervals)
			{
				summary.OfferedCalls += interval.OfferedCalls ?? 0;
				summary.HandleTime += interval.HandleTime ?? 0;
				summary.SpeedOfAnswer += interval.SpeedOfAnswer ?? 0;
				summary.AnsweredCalls += interval.AnsweredCalls ?? 0;
				summary.AnsweredCallsWithinSL += interval.AnsweredCallsWithinSL ?? 0;
				summary.AbandonedCalls += interval.AbandonedCalls ?? 0;

				timeSeries.Add(DateTime.MinValue.AddMinutes(interval.IntervalId * intervalLength));
				forecastedCallsSeries.Add(interval.ForecastedCalls);
				forecastedAverageHandleTimeSeries.Add(interval.ForecastedAverageHandleTime);
				offeredCallsSeries.Add(interval.OfferedCalls);
				averageHandleTimeSeries.Add(interval.OfferedCalls.HasValue ? interval.AverageHandleTime : null);

				if (interval.OfferedCalls.HasValue)
					latestQueueStatsIntervalId = interval.IntervalId;
				averageSpeedOfAnswer.Add(interval.SpeedOfAnswer.HasValue ? interval.SpeedOfAnswer / interval.AnsweredCalls : null);
				abandonedRate.Add(interval.AbandonedCalls.HasValue ? interval.AbandonedRate * 100 : null);
				serviceLevel.Add(interval.AnsweredCallsWithinSL.HasValue ? interval.ServiceLevel * 100 : null);

			}

			foreach (var interval in intervals.Where(interval => interval.IntervalId <= latestQueueStatsIntervalId))
			{
				summary.ForecastedCalls += interval.ForecastedCalls;
				summary.ForecastedHandleTime += interval.ForecastedHandleTime;
			}

			summary.ForecastedAverageHandleTime = Math.Abs(summary.ForecastedCalls) < 0.0001
					? 0
					: summary.ForecastedHandleTime / summary.ForecastedCalls;
			summary.AverageHandleTime = Math.Abs(summary.OfferedCalls) < 0.0001
								? 0
								: summary.HandleTime / summary.OfferedCalls;

			summary.ForecastedActualCallsDiff = Math.Abs(summary.ForecastedCalls) < 0.0001
				 ? -99
				 : (summary.OfferedCalls - summary.ForecastedCalls) * 100 / summary.ForecastedCalls;

			summary.ForecastedActualHandleTimeDiff = Math.Abs(summary.ForecastedAverageHandleTime) < 0.0001
					? -99
					: (summary.AverageHandleTime - summary.ForecastedAverageHandleTime) * 100 /
						summary.ForecastedAverageHandleTime;

			summary.AverageSpeedOfAnswer = Math.Abs(summary.AnsweredCalls) < 0.0001
					? -99
					: summary.SpeedOfAnswer / summary.AnsweredCalls;

			summary.ServiceLevel = Math.Abs(summary.OfferedCalls) < 0.0001
					? -99
					: summary.AnsweredCallsWithinSL / summary.OfferedCalls;

			summary.AbandonRate = Math.Abs(summary.OfferedCalls) < 0.0001
					? -99
					: summary.AbandonedCalls / summary.OfferedCalls;

			return new MonitorDataViewModel()
			{
				LatestActualIntervalStart = latestQueueStatsIntervalId == -1
		? null
		: (DateTime?)DateTime.MinValue.AddMinutes(latestQueueStatsIntervalId * intervalLength),
				LatestActualIntervalEnd = latestQueueStatsIntervalId == -1
		? null
		: (DateTime?)DateTime.MinValue.AddMinutes(latestQueueStatsIntervalId * intervalLength + intervalLength),
				Summary = summary,
				DataSeries = new MonitorIntradayDataSeries
				{
					Time = timeSeries.ToArray(),
					ForecastedCalls = forecastedCallsSeries.ToArray(),
					ForecastedAverageHandleTime = forecastedAverageHandleTimeSeries.ToArray(),
					OfferedCalls = offeredCallsSeries.ToArray(),
					AverageHandleTime = averageHandleTimeSeries.ToArray(),
					AverageSpeedOfAnswer = averageSpeedOfAnswer.ToArray(),
					AbandonedRate = abandonedRate.ToArray(),
					ServiceLevel = serviceLevel.ToArray()
				}
			};
		}
	}
}