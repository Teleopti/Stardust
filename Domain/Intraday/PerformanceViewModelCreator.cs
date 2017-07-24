using System;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class PerformanceViewModelCreator
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IncomingTrafficViewModelCreator _incomingTrafficViewModelCreator;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly EstimatedServiceLevelProvider _estimatedServiceLevelProvider;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;

		public PerformanceViewModelCreator(INow now,
			IUserTimeZone timeZone,
			IScenarioRepository scenarioRepository,
			IIntervalLengthFetcher intervalLengthFetcher,
			IncomingTrafficViewModelCreator incomingTrafficViewModelCreator,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			EstimatedServiceLevelProvider estimatedServiceLevelProvider,
			ISkillDayLoadHelper skillDayLoadHelper)
		{
			_now = now;
			_timeZone = timeZone;
			_scenarioRepository = scenarioRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
			_incomingTrafficViewModelCreator = incomingTrafficViewModelCreator;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_estimatedServiceLevelProvider = estimatedServiceLevelProvider;
			_skillDayLoadHelper = skillDayLoadHelper;
		}

		public IntradayPerformanceViewModel Load(Guid[] skillIdList)
		{
			return Load(skillIdList, _now.UtcDateTime());
		}

		public IntradayPerformanceViewModel Load(Guid[] skillIdList, int dayOffset)
		{
			return Load(skillIdList, _now.UtcDateTime().AddDays(dayOffset));
		}

		public IntradayPerformanceViewModel Load(Guid[] skillIdList, DateTime date)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var usersNow = TimeZoneHelper.ConvertFromUtc(date, _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			var skillDays = _skillDayLoadHelper.LoadSchedulerSkillDays(new DateOnlyPeriod(usersToday, usersToday.AddDays(1)), skills, scenario);
			var queueStatistics = _incomingTrafficViewModelCreator.Load(skillIdList, date);

			var eslIntervals = _estimatedServiceLevelProvider.CalculateEslIntervals(skillDays, queueStatistics, minutesPerInterval, date);

			return new IntradayPerformanceViewModel
			{
				LatestActualIntervalStart = queueStatistics.LatestActualIntervalStart,
				LatestActualIntervalEnd = queueStatistics.LatestActualIntervalEnd,
				DataSeries = new IntradayPerformanceDataSeries
				{
					Time = queueStatistics.DataSeries.Time,
					EstimatedServiceLevels = _estimatedServiceLevelProvider.DataSeries(eslIntervals, queueStatistics, minutesPerInterval),
					AverageSpeedOfAnswer = queueStatistics.DataSeries.AverageSpeedOfAnswer,
					AbandonedRate = queueStatistics.DataSeries.AbandonedRate,
					ServiceLevel = queueStatistics.DataSeries.ServiceLevel
				},
				Summary = new IntradayPerformanceSummary
				{
					AbandonRate = queueStatistics.Summary.AbandonRate,
					AverageSpeedOfAnswer = queueStatistics.Summary.AverageSpeedOfAnswer,
					ServiceLevel = queueStatistics.Summary.ServiceLevel,
					EstimatedServiceLevel = _estimatedServiceLevelProvider.EslSummary(eslIntervals)
				},
				PerformanceHasData = queueStatistics.DataSeries.CalculatedCalls.Any(x => x.HasValue)
			};
		}
	}
}