using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.Intraday.Domain;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer
{
	public class IntradayIncomingTrafficApplicationService
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IntradayStatisticsService _intradayStatisticsService;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly ISkillTypeInfoProvider _skillTypeInfoProvider;

		public IntradayIncomingTrafficApplicationService(
			INow now, 
			IUserTimeZone timeZone,
			IIntervalLengthFetcher intervalLengthFetcher,
			IntradayStatisticsService intradayStatisticsService,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			ISkillTypeInfoProvider skillTypeInfoProvider
			)
		{
			_now = now ?? throw new ArgumentNullException(nameof(now));
			_timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
			_intervalLengthFetcher = intervalLengthFetcher ?? throw new ArgumentNullException(nameof(intervalLengthFetcher));
			_intradayStatisticsService = intradayStatisticsService ?? throw new ArgumentNullException(nameof(intradayStatisticsService));
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider ?? throw new ArgumentNullException(nameof(supportedSkillsInIntradayProvider));
			_skillTypeInfoProvider = skillTypeInfoProvider ?? throw new ArgumentNullException(nameof(skillTypeInfoProvider));
		}

		public IntradayIncomingViewModel GenerateIncomingTrafficViewModel(Guid[] skillIdList)
		{
			return GenerateIncomingTrafficViewModel(skillIdList, _now.UtcDateTime());
		}

		public IntradayIncomingViewModel GenerateIncomingTrafficViewModel(Guid[] skillIdList, int dayOffset)
		{
			return GenerateIncomingTrafficViewModel(skillIdList, _now.UtcDateTime().AddDays(dayOffset));
		}
		public IntradayIncomingViewModel GenerateIncomingTrafficViewModel(Guid[] skillIdList, DateTime timeUtc)
		{
			var startOfDayLocal = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, _timeZone.TimeZone()).Date;
			var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayLocal.Date, _timeZone.TimeZone());

			var vm = new IntradayIncomingViewModel();
			var intervalLength = _intervalLengthFetcher.IntervalLength;

			var supportedSkills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			var abandonRateSupported = supportedSkills.All(x => _skillTypeInfoProvider.GetSkillTypeInfo(x).SupportsAbandonRate);

			var intervals = _intradayStatisticsService.GetIntervalsInUTC(supportedSkills.Select(x => x.Id.Value).ToArray(), startOfDayUtc, startOfDayUtc.AddDays(1));
			var summary = _intradayStatisticsService.GenerateStatisticsSummary(intervals, abandonRateSupported);

			var orderedIntervals = intervals
				.Select(x => new
				{
					IntervalTime = x.IntervalDate.Date.AddMinutes(x.IntervalId * intervalLength),
					x.ForecastedCalls,
					x.ForecastedAverageHandleTime,
					x.CalculatedCalls,
					AverageHandleTime = x.CalculatedCalls.HasValue ? x.AverageHandleTime : null,
					AverageSpeedOfAnswer = x.AnsweredCalls.HasValue ? x.AverageSpeedOfAnswer : null,
					AbandonedRate =
						x.AbandonedCalls.HasValue && abandonRateSupported ? x.AbandonedRate * 100 : null,
					ServiceLevel = x.AnsweredCallsWithinSL.HasValue
						? (double?) Math.Min(x.ServiceLevel.Value * 100, 100)
						: null
				})
				.OrderBy(x => x.IntervalTime);

			var forecastedCalls = new List<double>();
			var forecastedAverageHandleTime = new List<double>();
			var calculatedCalls = new List<double?>();
			var averageHandleTime = new List<double?>();
			var averageSpeedOfAnswer = new List<double?>();
			var abandonedRate = new List<double?>();
			var serviceLevel = new List<double?>();
			var timesLocal = new DateTime[]{};

			if (orderedIntervals.Any())
			{
				var firstIntervalStartTimeLocal =
					TimeZoneInfo.ConvertTimeFromUtc(orderedIntervals.Min(x => x.IntervalTime), _timeZone.TimeZone());
				
				var lastIntervalStartTimeLocal =
					TimeZoneInfo.ConvertTimeFromUtc(orderedIntervals.Max(x => x.IntervalTime), _timeZone.TimeZone());

				timesLocal = this.GenerateTimeSeries(firstIntervalStartTimeLocal, lastIntervalStartTimeLocal,
					intervalLength);


				foreach (var t in timesLocal)
				{
					var correspondingUtcTime = TimeZoneInfo.ConvertTimeToUtc(t, _timeZone.TimeZone());
					var correspondingorderedIntervals =
						orderedIntervals.Where(x => x.IntervalTime.Equals(correspondingUtcTime));

					forecastedCalls.Add(correspondingorderedIntervals.Any()
						? correspondingorderedIntervals.Sum(x => x.ForecastedCalls)
						: 0);
					forecastedAverageHandleTime.Add(correspondingorderedIntervals.Any()
						? correspondingorderedIntervals.Sum(x => x.ForecastedAverageHandleTime)
						: 0);

					calculatedCalls.Add(correspondingorderedIntervals.Any(x => x.CalculatedCalls.HasValue)
						? (double?) correspondingorderedIntervals.Where(x => x.CalculatedCalls.HasValue)
							.Sum(x => x.CalculatedCalls.Value)
						: null);

					averageHandleTime.Add(correspondingorderedIntervals.Any(x => x.AverageHandleTime.HasValue)
						? (double?) correspondingorderedIntervals.Where(x => x.AverageHandleTime.HasValue)
							.Sum(x => x.AverageHandleTime.Value)
						: null);

					averageSpeedOfAnswer.Add(
						correspondingorderedIntervals.Any(x => x.AverageSpeedOfAnswer.HasValue)
							? (double?) correspondingorderedIntervals.Where(x => x.AverageSpeedOfAnswer.HasValue)
								.Sum(x => x.AverageSpeedOfAnswer.Value)
							: null);

					abandonedRate.Add(correspondingorderedIntervals.Any(x => x.AbandonedRate.HasValue)
						? (double?) correspondingorderedIntervals.Where(x => x.AbandonedRate.HasValue)
							.Sum(x => x.AbandonedRate.Value)
						: null);

					serviceLevel.Add(correspondingorderedIntervals.Any(x => x.ServiceLevel.HasValue)
						? (double?) correspondingorderedIntervals.Where(x => x.ServiceLevel.HasValue)
							.Sum(x => x.ServiceLevel.Value)
						: null);
				}
			}

			var firstIntervalStartUtc = orderedIntervals?.FirstOrDefault()?.IntervalTime;
			firstIntervalStartUtc = firstIntervalStartUtc.HasValue
				? (DateTime?) TimeZoneInfo.ConvertTimeFromUtc(firstIntervalStartUtc.Value, _timeZone.TimeZone())
				: null;

			var latestActualIntervalStartUtc = orderedIntervals?.LastOrDefault(x => x.CalculatedCalls.HasValue)?.IntervalTime;
			latestActualIntervalStartUtc = latestActualIntervalStartUtc.HasValue
				? (DateTime?) TimeZoneInfo.ConvertTimeFromUtc(latestActualIntervalStartUtc.Value, _timeZone.TimeZone())
				: null;
			
			return new IntradayIncomingViewModel()
			{
				FirstIntervalStart = firstIntervalStartUtc,
				FirstIntervalEnd = firstIntervalStartUtc.HasValue 
					? (DateTime ?) firstIntervalStartUtc.Value.AddMinutes(intervalLength) : null,
				LatestActualIntervalStart = latestActualIntervalStartUtc,
				LatestActualIntervalEnd = latestActualIntervalStartUtc.HasValue 
					? (DateTime ?) latestActualIntervalStartUtc.Value.AddMinutes(intervalLength) : null,
				Summary = summary,
				DataSeries = new IntradayIncomingDataSeries
				{
					Time = timesLocal,
					ForecastedCalls = forecastedCalls.ToArray(),
					ForecastedAverageHandleTime = forecastedAverageHandleTime.ToArray(),
					CalculatedCalls = calculatedCalls.ToArray(),
					AverageHandleTime = averageHandleTime.ToArray(),
					AverageSpeedOfAnswer = averageSpeedOfAnswer.ToArray(),
					AbandonedRate = abandonedRate.ToArray(),
					ServiceLevel = serviceLevel.ToArray()
				},
				IncomingTrafficHasData = intervals.Any()
			};
		}

		public DateTime[] GenerateTimeSeries(DateTime firstIntervalStarTime, DateTime lastIntervalStartTime, int resolution)
		{
			var lastIntervalEndTime = lastIntervalStartTime.AddMinutes(resolution);
			var times = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(lastIntervalEndTime - firstIntervalStarTime).TotalMinutes / resolution))
				.Select(offset => firstIntervalStarTime.AddMinutes(offset * resolution))
				.ToArray();
			return times;
		}
	}
}
	
