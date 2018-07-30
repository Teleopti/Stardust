﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public interface IIntradayStatisticsService
	{
		IEnumerable<IncomingIntervalModel> GetIntervalsInUTC(Guid[] skillIdList, DateTime fromTimeInUTC, DateTime toTimeInUTC);

		IntradayIncomingSummary GenerateStatisticsSummary(IEnumerable<IncomingIntervalModel> intervals,
			bool abandonRateSupported);

		IList<SkillIntervalStatistics> GetSkillDayStatistics(
			IList<SkillDayStatsRange> skillDayStatsRange, 
			ISkill skill,
			DateOnly skillDayDate, 
			List<SkillIntervalStatistics> actualStatsPerInterval, 
			TimeSpan resolution);
	}

	public class IntradayStatisticsService : IIntradayStatisticsService
	{
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly IIntradayMonitorDataLoader _intradayMonitorDataLoader;

		public IntradayStatisticsService(
			IIntervalLengthFetcher intervalLengthFetcher,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			IIntradayMonitorDataLoader intradayMonitorDataLoader
		)
		{
			_intervalLengthFetcher = intervalLengthFetcher ?? throw new ArgumentNullException(nameof(intervalLengthFetcher));
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider ?? throw new ArgumentNullException(nameof(supportedSkillsInIntradayProvider));
			_intradayMonitorDataLoader = intradayMonitorDataLoader ?? throw new ArgumentNullException(nameof(intradayMonitorDataLoader));
		}

		public IEnumerable<IncomingIntervalModel> GetIntervalsInUTC(Guid[] skillIdList, DateTime fromTimeInUTC, DateTime toTimeInUTC)
		{
			var supportedSkills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			var intervalLength = _intervalLengthFetcher.IntervalLength;

			int numberOfDays = (int)Math.Ceiling((toTimeInUTC - fromTimeInUTC).TotalDays);

			var intervals = new List<IncomingIntervalModel>();
			for (int i = 0; i <= numberOfDays; i++)
			{
				var dayToLoad = new DateOnly(fromTimeInUTC.Date.AddDays(i));

				intervals.AddRange(_intradayMonitorDataLoader.Load(supportedSkills.Select(x => x.Id.Value).ToList(), TimeZoneInfo.Utc, dayToLoad));
			}

			var selected = intervals
				.Where(x =>
				{
					var intervalTime = x.IntervalDate.AddMinutes(x.IntervalId * intervalLength);
					return (intervalTime >= fromTimeInUTC && intervalTime < toTimeInUTC);
				})
				.OrderBy(y => y.IntervalId);
			return selected;
		}
		public IntradayIncomingSummary GenerateStatisticsSummary(IEnumerable<IncomingIntervalModel> intervals, bool abandonRateSupported)
		{
			var summary = new IntradayIncomingSummary();
			if (intervals.Any())
			{
				int latestQueueStatsIntervalId = -1;
				foreach (var interval in intervals)
				{
					summary.CalculatedCalls += interval.CalculatedCalls ?? 0;
					summary.HandleTime += interval.HandleTime ?? 0;
					summary.SpeedOfAnswer += interval.SpeedOfAnswer ?? 0;
					summary.AnsweredCalls += interval.AnsweredCalls ?? 0;
					summary.AnsweredCallsWithinSL += interval.AnsweredCallsWithinSL ?? 0;
					summary.AbandonedCalls += interval.AbandonedCalls ?? 0;

					if (interval.CalculatedCalls.HasValue)
					{
						latestQueueStatsIntervalId = interval.IntervalId;
					}
				}

				foreach (var interval in intervals.Where(interval => interval.IntervalId <= latestQueueStatsIntervalId))
				{
					summary.ForecastedCalls += interval.ForecastedCalls;
					summary.ForecastedHandleTime += interval.ForecastedHandleTime;
				}
			}

			summary.ForecastedAverageHandleTime =
				(Math.Abs(summary.ForecastedCalls) < 0.0001)
				? 0
				: summary.ForecastedHandleTime / summary.ForecastedCalls;

			summary.AverageHandleTime =
				(Math.Abs(summary.AnsweredCalls) < 0.0001)
				? 0
				: summary.HandleTime / summary.AnsweredCalls;

			summary.ForecastedActualCallsDiff =
				(Math.Abs(summary.ForecastedCalls) < 0.0001)
				? -99
				: (summary.CalculatedCalls - summary.ForecastedCalls) * 100 / summary.ForecastedCalls;

			summary.ForecastedActualHandleTimeDiff =
				(Math.Abs(summary.ForecastedAverageHandleTime) < 0.0001)
				? -99
				: (summary.AverageHandleTime - summary.ForecastedAverageHandleTime) * 100 / summary.ForecastedAverageHandleTime;

			summary.AverageSpeedOfAnswer =
				(Math.Abs(summary.AnsweredCalls) < 0.0001)
				? -99
				: summary.SpeedOfAnswer / summary.AnsweredCalls;

			summary.ServiceLevel =
				(Math.Abs(summary.CalculatedCalls) < 0.0001)
				? -99
				: Math.Min(summary.AnsweredCallsWithinSL / summary.CalculatedCalls, 1);

			summary.AbandonRate =
				(Math.Abs(summary.CalculatedCalls) < 0.0001 || !abandonRateSupported)
				? -99
				: summary.AbandonedCalls / summary.CalculatedCalls;

			return summary;
		}
		public IList<SkillIntervalStatistics> GetSkillDayStatistics(IList<SkillDayStatsRange> skillDayStatsRange, ISkill skill, DateOnly skillDayDate, List<SkillIntervalStatistics> actualStatsPerInterval, TimeSpan resolution)
		{
			var rangeStartUtc = skillDayStatsRange
				.FirstOrDefault(x => x.SkillId == skill.Id.Value && x.SkillDayDate == skillDayDate)?
				.RangePeriod.StartDateTime;
			if (!rangeStartUtc.HasValue)
				return new List<SkillIntervalStatistics>();
			var rangeEndUtc = skillDayStatsRange
				.FirstOrDefault(x => x.SkillId == skill.Id.Value && x.SkillDayDate == skillDayDate)?
				.RangePeriod.EndDateTime;
			var rangeStartLocal = rangeStartUtc.Value;
			var rangeEndLocal = rangeEndUtc.Value;

			var retList = actualStatsPerInterval
				.Where(x => x.StartTime >= rangeStartLocal && x.StartTime < rangeEndLocal)
				.ToList();

			if (skill.DefaultResolution == resolution.TotalMinutes)
				return retList;

			var intervalsInResolution = skill.DefaultResolution / resolution.TotalMinutes;
			var mergedStats = new List<SkillIntervalStatistics>();
			for (var interval = rangeStartLocal; interval < rangeEndLocal; interval = interval.AddMinutes(skill.DefaultResolution))
			{
				var statsPerWorkload = retList
					.Where(x => x.StartTime >= interval && x.StartTime < interval.AddMinutes(skill.DefaultResolution))
					.GroupBy(x => x.WorkloadId);
				foreach (var item in statsPerWorkload)
				{
					var itemCount = item.Count();
					var sumAnsweredCalls = item.Sum(x => x.AnsweredCalls);

					var averageCalls = (itemCount > 0 ? item.Sum(x => x.Calls) / itemCount : 0);
					var averageAht = (sumAnsweredCalls > 0 ? item.Sum(x => x.HandleTime) / sumAnsweredCalls : 0);

					foreach (var statInterval in item)
					{
						mergedStats.Add(new SkillIntervalStatistics
						{
							Calls = averageCalls * intervalsInResolution,
							AverageHandleTime = averageAht,
							SkillId = skill.Id.Value,
							StartTime = statInterval.StartTime,
							WorkloadId = item.Key
						});
					}
				}

			}
			return mergedStats;
		}
	}
}
