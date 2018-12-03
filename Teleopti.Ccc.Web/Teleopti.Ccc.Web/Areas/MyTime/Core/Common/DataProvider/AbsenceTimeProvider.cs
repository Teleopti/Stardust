using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AbsenceTimeProvider : IAbsenceTimeProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly IExtractBudgetGroupPeriods _extractBudgetGroupPeriod;
		private readonly IAbsenceTimeProviderCache _absenceTimeProviderCache;

		public AbsenceTimeProvider(	ILoggedOnUser loggedOnUser, 
									IScenarioRepository scenarioRepository, 
									IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, 
									IExtractBudgetGroupPeriods extractBudgetGroupPeriod, 
									IAbsenceTimeProviderCache absenceTimeProviderCache)
		{
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
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

			var absenceTimes = getAbsenceTimes(period, budgetGroup, scenario);
			fillAbsenceTimeInformationOnAbsenceAgents(target, absenceTimes);
		}

		private static void fillAbsenceTimeInformationOnAbsenceAgents(IEnumerable<AbsenceAgents> target, IEnumerable<PayloadWorkTime> absenceTime)
		{
			if (absenceTime == null) return;

			var absenceAgentses = target.ToLookup(t => t.Date);
			foreach (var payloadWorkTime in absenceTime)
			{
				var absenceAgent = absenceAgentses[payloadWorkTime.BelongsToDate].FirstOrDefault();
				if (absenceAgent == null)
				{
					continue;
				}
				absenceAgent.AbsenceTime += TimeSpan.FromTicks(payloadWorkTime.TotalContractTime).TotalMinutes;
				absenceAgent.HeadCounts += payloadWorkTime.HeadCounts;
			}
		}

		private IEnumerable<PayloadWorkTime> getAbsenceTimes(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			_absenceTimeProviderCache.Setup();

			var cacheKey = AbsenceTimeProviderCache.GetCacheKey(period, budgetGroup, scenario);
			var absenceTimes = _absenceTimeProviderCache.Get(cacheKey);
			if (absenceTimes != null) return absenceTimes;

			absenceTimes = getAbsenceTimeFromRepository(period, budgetGroup, scenario);
			_absenceTimeProviderCache.Add(cacheKey, absenceTimes);

			return absenceTimes;
		}

		private IEnumerable<PayloadWorkTime> getAbsenceTimeFromRepository(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			return _scheduleProjectionReadOnlyPersister.AbsenceTimePerBudgetGroup(period, budgetGroup, scenario);
		}	
	}
}



