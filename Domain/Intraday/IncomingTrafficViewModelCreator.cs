using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IncomingTrafficViewModelCreator
	{
		private readonly IIntradayMonitorDataLoader _intradayMonitorDataLoader;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly INow _now;
		private readonly IUserTimeZone _userTimeZone;
		private readonly SupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;

		public IncomingTrafficViewModelCreator(IIntradayMonitorDataLoader intradayMonitorDataLoader, IIntervalLengthFetcher intervalLengthFetcher, INow now, IUserTimeZone userTimeZone, SupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider)
		{
			_intradayMonitorDataLoader = intradayMonitorDataLoader;
			_intervalLengthFetcher = intervalLengthFetcher;
			_now = now;
			_userTimeZone = userTimeZone;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
		}

		public IntradayIncomingViewModel Load(Guid[] skillIdList)
		{
			var supportedSkills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);

			var intervals = _intradayMonitorDataLoader.Load(supportedSkills.Select(x => x.Id.Value).ToArray(),
				_userTimeZone.TimeZone(),
				new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _userTimeZone.TimeZone())));
			var intervalLength = _intervalLengthFetcher.IntervalLength;

			var summary = new IntradayIncomingSummary();
			var timeSeries = new List<DateTime>();
			var forecastedCallsSeries = new List<double>();
			var forecastedAverageHandleTimeSeries = new List<double>();
			var calculatedCallsSeries = new List<double?>();
			var averageHandleTimeSeries = new List<double?>();
			var latestQueueStatsIntervalId = -1;
			var latestQueueStatsIntervalDate = DateTime.MinValue;
			var averageSpeedOfAnswer = new List<double?>();
			var abandonedRate = new List<double?>();
			var serviceLevel = new List<double?>();

			foreach (var interval in intervals)
			{
				summary.CalculatedCalls += interval.CalculatedCalls ?? 0;
				summary.HandleTime += interval.HandleTime ?? 0;
				summary.SpeedOfAnswer += interval.SpeedOfAnswer ?? 0;
				summary.AnsweredCalls += interval.AnsweredCalls ?? 0;
				summary.AnsweredCallsWithinSL += interval.AnsweredCallsWithinSL ?? 0;
				summary.AbandonedCalls += interval.AbandonedCalls ?? 0;

				timeSeries.Add(interval.IntervalDate.AddMinutes(interval.IntervalId * intervalLength));
				forecastedCallsSeries.Add(interval.ForecastedCalls);
				forecastedAverageHandleTimeSeries.Add(interval.ForecastedAverageHandleTime);
				calculatedCallsSeries.Add(interval.CalculatedCalls);
				averageHandleTimeSeries.Add(interval.CalculatedCalls.HasValue ? interval.AverageHandleTime : null);

				if (interval.CalculatedCalls.HasValue)
				{
					latestQueueStatsIntervalId = interval.IntervalId;
					latestQueueStatsIntervalDate = interval.IntervalDate;
				}
					
				averageSpeedOfAnswer.Add(interval.AnsweredCalls.HasValue ? interval.AverageSpeedOfAnswer : null);
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
			summary.AverageHandleTime = Math.Abs(summary.AnsweredCalls) < 0.0001
								? 0
								: summary.HandleTime / summary.AnsweredCalls;

			summary.ForecastedActualCallsDiff = Math.Abs(summary.ForecastedCalls) < 0.0001
				 ? -99
				 : (summary.CalculatedCalls - summary.ForecastedCalls) * 100 / summary.ForecastedCalls;

			summary.ForecastedActualHandleTimeDiff = Math.Abs(summary.ForecastedAverageHandleTime) < 0.0001
					? -99
					: (summary.AverageHandleTime - summary.ForecastedAverageHandleTime) * 100 /
						summary.ForecastedAverageHandleTime;

			summary.AverageSpeedOfAnswer = Math.Abs(summary.AnsweredCalls) < 0.0001
					? -99
					: summary.SpeedOfAnswer / summary.AnsweredCalls;

			summary.ServiceLevel = Math.Abs(summary.CalculatedCalls) < 0.0001
					? -99
					: summary.AnsweredCallsWithinSL / summary.CalculatedCalls;

			summary.AbandonRate = Math.Abs(summary.CalculatedCalls) < 0.0001
					? -99
					: summary.AbandonedCalls / summary.CalculatedCalls;

			return new IntradayIncomingViewModel()
			{
				LatestActualIntervalStart = latestQueueStatsIntervalId == -1
					? null
					: (DateTime?) latestQueueStatsIntervalDate.AddMinutes(latestQueueStatsIntervalId*intervalLength),
				LatestActualIntervalEnd = latestQueueStatsIntervalId == -1
					? null
					: (DateTime?)latestQueueStatsIntervalDate.AddMinutes(latestQueueStatsIntervalId*intervalLength + intervalLength),
				Summary = summary,
				DataSeries = new IntradayIncomingDataSeries
				{
					Time = timeSeries.ToArray(),
					ForecastedCalls = forecastedCallsSeries.ToArray(),
					ForecastedAverageHandleTime = forecastedAverageHandleTimeSeries.ToArray(),
					CalculatedCalls = calculatedCallsSeries.ToArray(),
					AverageHandleTime = averageHandleTimeSeries.ToArray(),
					AverageSpeedOfAnswer = averageSpeedOfAnswer.ToArray(),
					AbandonedRate = abandonedRate.ToArray(),
					ServiceLevel = serviceLevel.ToArray()
				},
				IncomingTrafficHasData = intervals.Any()
			};
		}
	}
}