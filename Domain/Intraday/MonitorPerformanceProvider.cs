using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
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
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly MonitorSkillsProvider _monitorSkillsProvider;
		private readonly ForecastedCallsProvider _forecastedCallsProvider;
		private readonly SupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;

		public MonitorPerformanceProvider(INow now,
			IUserTimeZone timeZone,
			ISkillDayRepository skillDayRepository,
			IScenarioRepository scenarioRepository,
			IIntervalLengthFetcher intervalLengthFetcher,
			ScheduledStaffingProvider scheduledStaffingProvider,
			ForecastedStaffingProvider forecastedStaffingProvider,
			MonitorSkillsProvider monitorSkillsProvider,
			ForecastedCallsProvider forecastedCallsProvider,
			SupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider)
		{
			_now = now;
			_timeZone = timeZone;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_monitorSkillsProvider = monitorSkillsProvider;
			_forecastedCallsProvider = forecastedCallsProvider;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
		}
		public IntradayPerformanceViewModel Load(Guid[] skillIdList)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			var skillDays = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(usersToday, usersToday), skills, scenario);
			var queueStatistics = _monitorSkillsProvider.Load(skillIdList);

			var eslIntervals = getEsl(skills, skillDays, queueStatistics, minutesPerInterval);

			return new IntradayPerformanceViewModel
			{
				DataSeries = new IntradayPerformanceDataSeries
				{
					EstimatedServiceLevels = eslDataSeries(eslIntervals, queueStatistics),
					AverageSpeedOfAnswer = queueStatistics.StatisticsDataSeries.AverageSpeedOfAnswer,
					AbandonedRate = queueStatistics.StatisticsDataSeries.AbandonedRate,
					ServiceLevel = queueStatistics.StatisticsDataSeries.ServiceLevel
				},
				Summary = new IntradayPerformanceSummary
				{
					AbandonRate = queueStatistics.StatisticsSummary.AbandonRate,
					AverageSpeedOfAnswer = queueStatistics.StatisticsSummary.AverageSpeedOfAnswer,
					ServiceLevel = queueStatistics.StatisticsSummary.ServiceLevel
				}
			};
		}

		private IList<EslInterval> getEsl(IList<ISkill> skills, ICollection<ISkillDay> skillDays, IntradayStatisticsViewModel queueStatistics,
			int minutesPerInterval)
		{
			var eslIntervals = new List<EslInterval>();
			if (!skillDays.Any())
				return eslIntervals;

			var forecastedCalls = _forecastedCallsProvider.Load(skills, skillDays, queueStatistics.LatestActualIntervalStart, minutesPerInterval);

			var forecastedStaffing = _forecastedStaffingProvider.StaffingPerSkill(skills, skillDays, minutesPerInterval)
				.Where(x => x.StartTime <= queueStatistics.LatestActualIntervalStart)
				.ToList();

			var scheduledStaffing = _scheduledStaffingProvider.StaffingPerSkill(skills, minutesPerInterval);

			var serviceCalculatorService = new StaffingCalculatorServiceFacade();


			foreach (var skill in skills)
			{
				var skillForecastedStaffing = forecastedStaffing
					.Where(s => s.SkillId == skill.Id.Value).ToList();
				foreach (var interval in skillForecastedStaffing)
				{
					var scheduledStaffingInterval = scheduledStaffing
						.SingleOrDefault(x => x.Id == interval.SkillId && x.StartDateTime == interval.StartTime).StaffingLevel;
					var skillDay = skillDays
						.SingleOrDefault(x => x.Skill.Id.Value == skill.Id.Value);
					var skillData = skillDay.SkillDataPeriodCollection
						.SingleOrDefault(
							skillDataPeriod => skillDataPeriod.Period.StartDateTime <= interval.StartTime &&
													 skillDataPeriod.Period.EndDateTime > interval.StartTime
						);
					var task = forecastedCalls.CallsPerSkill[skill.Id.Value].FirstOrDefault(x => x.StartTime == interval.StartTime);
					var esl = serviceCalculatorService.ServiceLevelAchievedOcc(
						scheduledStaffingInterval,
						skillData.ServiceLevelSeconds,
						task.Calls,
						task.AverageHandleTime,
						TimeSpan.FromMinutes(minutesPerInterval), 
						skillData.ServiceLevelPercent.Value, 
						interval.Agents, 
						1);

					eslIntervals.Add(new EslInterval
					{
						StartTime = interval.StartTime,
						ForecastedCalls = task.Calls,
						Esl = esl
					});
				}
			}

			return eslIntervals
				.GroupBy(g => g.StartTime)
				.Select(s => new EslInterval
				{
					StartTime = s.Key,
					ForecastedCalls = s.Sum(x => x.ForecastedCalls),
					Esl = s.Sum(x => x.AnsweredCallsWithinServiceLevel)/s.Sum(x => x.ForecastedCalls)
				})
				.ToList();
		}

		private double?[] eslDataSeries(IList<EslInterval> eslIntervals, IntradayStatisticsViewModel queueStatistics)
		{
			var dataSeries = eslIntervals
				.Select(x => (double?) x.Esl)
				.ToList();
			var nullCount = queueStatistics.StatisticsDataSeries.Time.Length - dataSeries.Count;
			for (int interval = 0; interval < nullCount; interval++)
				dataSeries.Add(null);

			return dataSeries.ToArray();
		}
	}
}