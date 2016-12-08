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
			var skillDays = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(usersToday.AddDays(-1), usersToday.AddDays(1)), skills, scenario);
			var queueStatistics = _monitorSkillsProvider.Load(skillIdList);

			var eslIntervals = getEsl(skills, skillDays, queueStatistics, minutesPerInterval);

			return new IntradayPerformanceViewModel
			{
				DataSeries = new IntradayPerformanceDataSeries
				{
					Time = queueStatistics.IncomingDataSeries.Time,
					EstimatedServiceLevels = eslDataSeries(eslIntervals, queueStatistics),
					AverageSpeedOfAnswer = queueStatistics.IncomingDataSeries.AverageSpeedOfAnswer,
					AbandonedRate = queueStatistics.IncomingDataSeries.AbandonedRate,
					ServiceLevel = queueStatistics.IncomingDataSeries.ServiceLevel
				},
				Summary = new IntradayPerformanceSummary
				{
					AbandonRate = queueStatistics.IncomingSummary.AbandonRate,
					AverageSpeedOfAnswer = queueStatistics.IncomingSummary.AverageSpeedOfAnswer,
					ServiceLevel = queueStatistics.IncomingSummary.ServiceLevel,
					EstimatedServiceLevel = getEslSummary(eslIntervals)
				}
			};
		}

		private double getEslSummary(IList<EslInterval> eslIntervals)
		{
			var sumOfForecastedCalls = eslIntervals.Sum(x => x.ForecastedCalls);
			var sumOfAnsweredCallsWithinSL = eslIntervals.Sum(x => x.AnsweredCallsWithinServiceLevel);
			return sumOfAnsweredCallsWithinSL/sumOfForecastedCalls;
		}

		private IList<EslInterval> getEsl(IList<ISkill> skills, ICollection<ISkillDay> skillDays, IntradayIncomingViewModel queueStatistics,
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
					var intervalStartTimeUtc = TimeZoneHelper.ConvertToUtc(interval.StartTime, _timeZone.TimeZone());
					var scheduledStaffingInterval = scheduledStaffing
						.SingleOrDefault(x => x.Id == interval.SkillId && x.StartDateTime == interval.StartTime).StaffingLevel;
					var skillDaysForSkill = skillDays
						.Where(x => x.Skill.Id.Value == skill.Id.Value);

					ISkillDataPeriod skillData = null;
					foreach (var skillDay in skillDaysForSkill)
					{
						skillData = skillDay.SkillDataPeriodCollection
						.SingleOrDefault(
							skillDataPeriod => skillDataPeriod.Period.StartDateTime <= intervalStartTimeUtc &&
													 skillDataPeriod.Period.EndDateTime > intervalStartTimeUtc
						);
						if (skillData != null)
							break;
					}

					if (skillData == null)
						continue;

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

		private double?[] eslDataSeries(IList<EslInterval> eslIntervals, IntradayIncomingViewModel queueIncoming)
		{
			var dataSeries = eslIntervals
				.Select(x => (double?) x.Esl)
				.ToList();
			var nullCount = queueIncoming.IncomingDataSeries.Time.Length - dataSeries.Count;
			for (int interval = 0; interval < nullCount; interval++)
				dataSeries.Add(null);

			return dataSeries.ToArray();
		}
	}
}