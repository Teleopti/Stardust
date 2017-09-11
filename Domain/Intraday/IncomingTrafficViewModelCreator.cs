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
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly ISkillTypeInfoProvider _skillTypeInfoProvider;

		public IncomingTrafficViewModelCreator(IIntradayMonitorDataLoader intradayMonitorDataLoader,
			IIntervalLengthFetcher intervalLengthFetcher, INow now, IUserTimeZone userTimeZone,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider, ISkillTypeInfoProvider skillTypeInfoProvider)
		{
			_intradayMonitorDataLoader = intradayMonitorDataLoader;
			_intervalLengthFetcher = intervalLengthFetcher;
			_now = now;
			_userTimeZone = userTimeZone;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_skillTypeInfoProvider = skillTypeInfoProvider;
		}

		public IntradayIncomingViewModel Load(Guid[] skillIdList)
		{
			return Load(skillIdList, _now.UtcDateTime());
		}

		public IntradayIncomingViewModel Load(Guid[] skillIdList, int dayOffset)
		{
			return Load(skillIdList, _now.UtcDateTime().AddDays(dayOffset));
		}

		public IntradayIncomingViewModel Load(Guid[] skillIdList, DateTime utcDate)
		{
			var supportedSkills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			var abandonRateSupported = supportedSkills.All(x => _skillTypeInfoProvider.GetSkillTypeInfo(x).SupportsAbandonRate);

			var intervals = _intradayMonitorDataLoader.Load(supportedSkills.Select(x => x.Id.Value).ToArray(),
				_userTimeZone.TimeZone(),
				new DateOnly(TimeZoneHelper.ConvertFromUtc(utcDate, _userTimeZone.TimeZone())));

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
				abandonedRate.Add(interval.AbandonedCalls.HasValue && abandonRateSupported ? interval.AbandonedRate * 100 : null);

				serviceLevel.Add(interval.AnsweredCallsWithinSL.HasValue ? (double?)Math.Min(interval.ServiceLevel.Value * 100, 100): null);
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
					: Math.Min(summary.AnsweredCallsWithinSL / summary.CalculatedCalls, 1);

			summary.AbandonRate = Math.Abs(summary.CalculatedCalls) < 0.0001 || !abandonRateSupported
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