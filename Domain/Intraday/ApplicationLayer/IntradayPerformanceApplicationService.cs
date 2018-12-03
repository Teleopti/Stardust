using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer
{
	public interface IIntradayPerformanceApplicationService
	{
		IntradayPerformanceViewModel GeneratePerformanceViewModel(Guid[] skillIdList);
		IntradayPerformanceViewModel GeneratePerformanceViewModel(Guid[] skillIdList, int dayOffset);
		IntradayPerformanceViewModel GeneratePerformanceViewModel(Guid[] skillIdList, DateTime nowUtc);
	}

	public class IntradayPerformanceApplicationService : IIntradayPerformanceApplicationService
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly ISkillTypeInfoProvider _skillTypeInfoProvider;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly IIntradayStatisticsService _statisticsService;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IIntradayForecastingService _forecastingService;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;

		public IntradayPerformanceApplicationService(
			INow now,
			IUserTimeZone timeZone,
			IIntervalLengthFetcher intervalLengthFetcher,
			ISkillTypeInfoProvider skillTypeInfoProvider,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			IIntradayStatisticsService statisticsService,
			IScenarioRepository scenarioRepository,
			ISkillDayLoadHelper skillDayLoadHelper,
			IIntradayForecastingService forecastingService)
		{
			_now = now ?? throw new ArgumentNullException(nameof(now));
			_timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
			_intervalLengthFetcher = intervalLengthFetcher ?? throw new ArgumentNullException(nameof(intervalLengthFetcher));
			_skillTypeInfoProvider = skillTypeInfoProvider ?? throw new ArgumentNullException(nameof(skillTypeInfoProvider));
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider ?? throw new ArgumentNullException(nameof(supportedSkillsInIntradayProvider));
			_statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
			_scenarioRepository = scenarioRepository ?? throw new ArgumentNullException(nameof(scenarioRepository));
			_skillDayLoadHelper = skillDayLoadHelper ?? throw new ArgumentNullException(nameof(skillDayLoadHelper));

			_forecastingService = forecastingService ?? throw new ArgumentNullException(nameof(forecastingService));
		}

		public IntradayPerformanceViewModel GeneratePerformanceViewModel(Guid[] skillIdList)
		{
			return GeneratePerformanceViewModel(skillIdList, _now.UtcDateTime());
		}

		public IntradayPerformanceViewModel GeneratePerformanceViewModel(Guid[] skillIdList, int dayOffset)
		{
			return GeneratePerformanceViewModel(skillIdList, _now.UtcDateTime().AddDays(dayOffset));
		}

		public IntradayPerformanceViewModel GeneratePerformanceViewModel(Guid[] skillIdList, DateTime nowUtc)
		{
			var intervalLength = _intervalLengthFetcher.IntervalLength;
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			var abandonRateSupported = skills.All(x => _skillTypeInfoProvider.GetSkillTypeInfo(x).SupportsAbandonRate);

			var usersNowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _timeZone.TimeZone());
			var startOfDayLocal = usersNowLocal.Date;
			var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayLocal, _timeZone.TimeZone());
			var endOfDayUtc = startOfDayUtc.AddDays(1);

			var scenario = _scenarioRepository.LoadDefaultScenario();

			var skillDays = _skillDayLoadHelper.LoadSchedulerSkillDaysFlat(
				new DateOnlyPeriod(new DateOnly(startOfDayUtc), new DateOnly(startOfDayUtc.AddDays(1))), 
				skills, 
				scenario);

			var statisticsSumary = new IntradayIncomingSummary();
			DateTime? timeOfLastIntervalWithDataUtc = null;
			var intervalTimesUtc = new List<DateTime>();
			var intervalsInUTC = _statisticsService.GetIntervalsInUTC(skillIdList, startOfDayUtc, startOfDayUtc.AddDays(1));
			if (intervalsInUTC.Any())
			{
				statisticsSumary = _statisticsService.GenerateStatisticsSummary(intervalsInUTC, abandonRateSupported);

				intervalTimesUtc = intervalsInUTC
					.Select(x => x.IntervalDate.AddMinutes(x.IntervalId * intervalLength))
					.OrderBy(x => x)
					.ToList();

				timeOfLastIntervalWithDataUtc = intervalsInUTC
					.Where(x => x.CalculatedCalls.HasValue)
					.Select(x => x.IntervalDate.AddMinutes(x.IntervalId * intervalLength))
					.LastOrDefault();
			}

			var opensAtUtc = DateTime.SpecifyKind(intervalTimesUtc.Any() ? intervalTimesUtc.First() : startOfDayUtc, DateTimeKind.Utc);
			var closeAtUtc = DateTime.SpecifyKind(intervalTimesUtc.Any() ? intervalTimesUtc.Last().AddMinutes(intervalLength) : endOfDayUtc, DateTimeKind.Utc);

			var timesUtc = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(closeAtUtc - opensAtUtc).TotalMinutes / intervalLength))
				.Select(offset => opensAtUtc.AddMinutes(offset * intervalLength))
				.ToArray();

			var opensAtLocal = TimeZoneHelper.ConvertFromUtc(opensAtUtc, _timeZone.TimeZone());
			var closeAtLocal = TimeZoneHelper.ConvertFromUtc(closeAtUtc, _timeZone.TimeZone());
			var timesInLocal = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(closeAtLocal - opensAtLocal).TotalMinutes / intervalLength))
				.Select(offset => opensAtLocal.AddMinutes(offset * intervalLength))
				.ToArray();

			var endTimeForEslUtc = DateTime.SpecifyKind(
				((timeOfLastIntervalWithDataUtc.HasValue && timeOfLastIntervalWithDataUtc.Value != default(DateTime) ) ? timeOfLastIntervalWithDataUtc.Value.AddMinutes(intervalLength) : nowUtc), 
				DateTimeKind.Utc);
			var eslIntervals = _forecastingService.CalculateEstimatedServiceLevels(skillDays, opensAtUtc, endTimeForEslUtc);

			var estServiceLevels = new List<double?>();
			if (eslIntervals != null && eslIntervals.Any())
			{
				estServiceLevels = timesUtc
				.Select(x => new
				{
					LocalTime = TimeZoneInfo.ConvertTimeFromUtc(x, _timeZone.TimeZone()),
					Esl = eslIntervals.Any(e => e.StartTime == x && e.Esl.HasValue) ? eslIntervals.Where(e => e.StartTime == x && e.Esl.HasValue).Sum(e => e.Esl * 100) : null
				})
				.GroupBy(x => x.LocalTime)
				.Select(x => x.Any(e => e.Esl.HasValue) ? x.Where(e => e.Esl.HasValue).Sum(e => e.Esl) / x.Count() : null)
				.ToList();

				// estServiceLevels = estServiceLevels.Take(estServiceLevels.FindLastIndex(x => x.HasValue) + 1).ToList();
			}
			DateTime? timeOfLastIntervalWithData = timeOfLastIntervalWithDataUtc.HasValue ? (DateTime?)TimeZoneHelper.ConvertFromUtc(timeOfLastIntervalWithDataUtc.Value, _timeZone.TimeZone()) : null;

			var sumOfForecastedCalls = eslIntervals.Sum(x => x.ForecastedCalls);
			return new IntradayPerformanceViewModel
			{
				LatestActualIntervalStart = timeOfLastIntervalWithData,
				LatestActualIntervalEnd = timeOfLastIntervalWithDataUtc.HasValue ? (DateTime?)timeOfLastIntervalWithData.Value.AddMinutes(intervalLength) : null,
				DataSeries = new IntradayPerformanceDataSeries
				{
					Time = timesInLocal.ToArray(),
					EstimatedServiceLevels = estServiceLevels.ToArray(),
					AverageSpeedOfAnswer = intervalsInUTC.Select(x => x.AnsweredCalls.HasValue ? x.AverageSpeedOfAnswer : null).ToArray(),
					AbandonedRate = intervalsInUTC.Select(x => x.AbandonedCalls.HasValue && abandonRateSupported ? x.AbandonedRate * 100 : null).ToArray(),
					ServiceLevel = intervalsInUTC.Select(x => x.AnsweredCallsWithinSL.HasValue ? (double?)Math.Min(x.ServiceLevel.Value * 100, 100) : null).ToArray()
				},
				Summary = new IntradayPerformanceSummary
				{
					AbandonRate = statisticsSumary.AbandonRate,
					AverageSpeedOfAnswer = statisticsSumary.AverageSpeedOfAnswer,
					ServiceLevel = statisticsSumary.ServiceLevel,
					EstimatedServiceLevel = Math.Abs(sumOfForecastedCalls) > 0.01 
						? eslIntervals.Sum(x => x.AnsweredCallsWithinServiceLevel) / sumOfForecastedCalls * 100 ?? 0 : 0
				},
				PerformanceHasData = intervalsInUTC.Any(x => x.CalculatedCalls.HasValue)
			};
		}
	}
}
