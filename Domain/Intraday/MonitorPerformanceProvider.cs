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
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly MonitorSkillsProvider _monitorSkillsProvider;
		private readonly ForecastedCallsProvider _forecastedCallsProvider;

		public MonitorPerformanceProvider(INow now,
			IUserTimeZone timeZone,
			ISkillRepository skillRepository,
			ISkillDayRepository skillDayRepository,
			IScenarioRepository scenarioRepository,
			IIntervalLengthFetcher intervalLengthFetcher,
			ScheduledStaffingProvider scheduledStaffingProvider,
			ForecastedStaffingProvider forecastedStaffingProvider,
			MonitorSkillsProvider monitorSkillsProvider,
			ForecastedCallsProvider forecastedCallsProvider)
		{
			_now = now;
			_timeZone = timeZone;
			_skillRepository = skillRepository;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_monitorSkillsProvider = monitorSkillsProvider;
			_forecastedCallsProvider = forecastedCallsProvider;
		}
		public IntradayPerformanceDataSeries Load(Guid[] skillIdList)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _skillRepository.LoadSkills(skillIdList);
			var skillDay = _skillDayRepository.FindRange(new DateOnlyPeriod(usersToday, usersToday), skills.First(), scenario).First();
			var queueStatistics = _monitorSkillsProvider.Load(new Guid[] {skills.First().Id.Value});
			var forecastedCalls = _forecastedCallsProvider.Load(new List<ISkill>() {skills.First()},
				new List<ISkillDay>() {skillDay}, queueStatistics.LatestActualIntervalStart, minutesPerInterval);

			var forecastedStaffing = _forecastedStaffingProvider.StaffingPerSkill(new List<ISkill>() {skills.First()}, 
				new List<ISkillDay>() {skillDay}, 
				minutesPerInterval);
			
			var scheduledStaffing = _scheduledStaffingProvider.StaffingPerSkill(new List<ISkill>() { skills.First() }, minutesPerInterval);
			var serviceCalculatorService = new StaffingCalculatorServiceFacade();
			var eslDataSeries = new List<double?>();

			foreach (var interval in forecastedStaffing)
			{				
				var scheduledStaffingInterval = scheduledStaffing.SingleOrDefault(x => x.StartDateTime == interval.StartTime).StaffingLevel;
				var skillData =
					skillDay.SkillDataPeriodCollection.SingleOrDefault(
						skillDataPeriod => skillDataPeriod.Period.StartDateTime <= interval.StartTime &&
										   skillDataPeriod.Period.EndDateTime > interval.StartTime
					);
				var task = forecastedCalls.CallsPerSkill.First().Value.First(x => x.StartTime == interval.StartTime);
				var esl = serviceCalculatorService.ServiceLevelAchievedOcc(scheduledStaffingInterval,
					skillData.ServiceLevelSeconds,
					task.Calls,
					task.AverageHandleTime,
					TimeSpan.FromMinutes(minutesPerInterval), skillData.ServiceLevelPercent.Value, interval.Agents, 1);
				eslDataSeries.Add(esl);
			}

			return new IntradayPerformanceDataSeries() {EstimatedServiceLevels = eslDataSeries.ToArray()};
		}
	}
}