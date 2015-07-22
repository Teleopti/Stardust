using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AbsenceTimeProvider : IAbsenceTimeProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
		private readonly IExtractBudgetGroupPeriods _extractBudgetGroupPeriod;
		private readonly IAbsenceTimeProviderCache _absenceTimeProviderCache;

		public AbsenceTimeProvider(	ILoggedOnUser loggedOnUser, 
									IScenarioRepository scenarioRepository, 
									IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, 
									IExtractBudgetGroupPeriods extractBudgetGroupPeriod, 
									IAbsenceTimeProviderCache absenceTimeProviderCache)
		{
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
			_extractBudgetGroupPeriod = extractBudgetGroupPeriod;
			_absenceTimeProviderCache = absenceTimeProviderCache;
		}

		public IEnumerable<IAbsenceAgents> GetAbsenceTimeForPeriod(DateOnlyPeriod period)
		{
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var absenceDays = period.DayCollection().Select(d => new AbsenceAgents { Date = d.Date, AbsenceTime = 0, HeadCounts = 0 }).ToList();

			var budgetGroupsPeriod = _extractBudgetGroupPeriod.BudgetGroupsForPeriod(_loggedOnUser.CurrentUser(), period);
			foreach (var tuple in budgetGroupsPeriod)
			{
				addTime(absenceDays, tuple.Item1, tuple.Item2, defaultScenario);
			}

			return absenceDays;
		}

		private void addTime(IEnumerable<AbsenceAgents> target, DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			if (budgetGroup == null) return;
			
			fillAbsenceTimeInformationOnAbsenceAgents(target, getAbsenceTimes(period, budgetGroup, scenario));
		}

		private static void fillAbsenceTimeInformationOnAbsenceAgents(IEnumerable<AbsenceAgents> target, IEnumerable<PayloadWorkTime> absenceTime)
		{
			if (absenceTime == null) return;

			var absenceAgentses = target as IList<AbsenceAgents> ?? target.ToList();
			foreach (var payloadWorkTime in absenceTime)
			{
				var absenceAgent = absenceAgentses.First(a => a.Date == payloadWorkTime.BelongsToDate);
				absenceAgent.AbsenceTime += TimeSpan.FromTicks(payloadWorkTime.TotalContractTime).TotalMinutes;
				absenceAgent.HeadCounts += payloadWorkTime.HeadCounts;
			}
		}

		private IEnumerable<PayloadWorkTime> getAbsenceTimes(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			_absenceTimeProviderCache.Setup (scenario, period, budgetGroup);

			var absenceTimes = _absenceTimeProviderCache.Get();
			if (absenceTimes != null) return absenceTimes;

			absenceTimes = getAbsenceTimeFromRepository(period, budgetGroup, scenario);
			_absenceTimeProviderCache.Add(absenceTimes);

			return absenceTimes;
		}

		private IEnumerable<PayloadWorkTime> getAbsenceTimeFromRepository(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			return _scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(period, budgetGroup, scenario);
		}	
	}
}



