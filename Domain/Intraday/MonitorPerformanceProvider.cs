using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class MonitorPerformanceProvider
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly MonitorSkillsProvider _monitorSkillsProvider;
		private readonly SupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly EstimatedServiceLevelProvider _estimatedServiceLevelProvider;

		public MonitorPerformanceProvider(INow now,
			IUserTimeZone timeZone,
			ISkillDayRepository skillDayRepository,
			IScenarioRepository scenarioRepository,
			IIntervalLengthFetcher intervalLengthFetcher,
			MonitorSkillsProvider monitorSkillsProvider,
			SupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			EstimatedServiceLevelProvider estimatedServiceLevelProvider)
		{
			_now = now;
			_timeZone = timeZone;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
			_monitorSkillsProvider = monitorSkillsProvider;
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
			var queueStatistics = _monitorSkillsProvider.Load(skillIdList);

			var eslIntervals = _estimatedServiceLevelProvider.CalculateEslIntervals(skills, skillDays, queueStatistics, minutesPerInterval);

			return new IntradayPerformanceViewModel
			{
				DataSeries = new IntradayPerformanceDataSeries
				{
					Time = queueStatistics.IncomingDataSeries.Time,
					EstimatedServiceLevels = _estimatedServiceLevelProvider.DataSeries(eslIntervals, queueStatistics),
					AverageSpeedOfAnswer = queueStatistics.IncomingDataSeries.AverageSpeedOfAnswer,
					AbandonedRate = queueStatistics.IncomingDataSeries.AbandonedRate,
					ServiceLevel = queueStatistics.IncomingDataSeries.ServiceLevel
				},
				Summary = new IntradayPerformanceSummary
				{
					AbandonRate = queueStatistics.IncomingSummary.AbandonRate,
					AverageSpeedOfAnswer = queueStatistics.IncomingSummary.AverageSpeedOfAnswer,
					ServiceLevel = queueStatistics.IncomingSummary.ServiceLevel,
					EstimatedServiceLevel = _estimatedServiceLevelProvider.EslSummary(eslIntervals)
				}
			};
		}
	}
}