using System;
using System.Linq;
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
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IncomingTrafficViewModelCreator _incomingTrafficViewModelCreator;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly EstimatedServiceLevelProvider _estimatedServiceLevelProvider;

		public PerformanceViewModelCreator(INow now,
			IUserTimeZone timeZone,
			ISkillDayRepository skillDayRepository,
			IScenarioRepository scenarioRepository,
			IIntervalLengthFetcher intervalLengthFetcher,
			IncomingTrafficViewModelCreator incomingTrafficViewModelCreator,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			EstimatedServiceLevelProvider estimatedServiceLevelProvider)
		{
			_now = now;
			_timeZone = timeZone;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
			_incomingTrafficViewModelCreator = incomingTrafficViewModelCreator;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_estimatedServiceLevelProvider = estimatedServiceLevelProvider;
		}
		public IntradayPerformanceViewModel Load(Guid[] skillIdList)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			var skillDays = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(usersToday.AddDays(-1), usersToday.AddDays(1)), skills, scenario);
			var queueStatistics = _incomingTrafficViewModelCreator.Load(skillIdList);

			var eslIntervals = _estimatedServiceLevelProvider.CalculateEslIntervals(skills, skillDays, queueStatistics, minutesPerInterval);

			return new IntradayPerformanceViewModel
			{
				LatestActualIntervalStart = queueStatistics.LatestActualIntervalStart,
				LatestActualIntervalEnd = queueStatistics.LatestActualIntervalEnd,
				DataSeries = new IntradayPerformanceDataSeries
				{
					Time = queueStatistics.DataSeries.Time,
					EstimatedServiceLevels = _estimatedServiceLevelProvider.DataSeries(eslIntervals, queueStatistics),
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