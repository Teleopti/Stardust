using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AbsenceTimeProvider : IAbsenceTimeProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly IExtractBudgetGroupPeriods _extractBudgetGroupPeriod;
		private readonly IAbsenceTimeProviderCache _absenceTimeProviderCache;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(AbsenceTimeProvider));

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
			if (fillAbsenceTimeInformationOnAbsenceAgents(target, absenceTimes))
			{
				var absenceAgentsDaysStr = $"period from {target.First().Date.ToShortDateString()} {target.First().Date.Kind} to {target.Last().Date.ToShortDateString()} {target.Last().Date.Kind}";
				var payloadWorkTimeStr = String.Join(", ", absenceTimes.Select(p => p.BelongsToDate.ToShortDateString()+" "+p.BelongsToDate.Kind));
				Logger.Warn($"Date mismatch: {absenceAgentsDaysStr} <-> {payloadWorkTimeStr} for budgetGroup: {budgetGroup.Id} and person: {_loggedOnUser.CurrentUser().Id} on scenario: {scenario.Id}");
			}
		}

		private static bool fillAbsenceTimeInformationOnAbsenceAgents(IEnumerable<AbsenceAgents> target, IEnumerable<PayloadWorkTime> absenceTime)
		{
			var hasMismatch = false;
			if (absenceTime == null) return false;


			var absenceAgentses = target as IList<AbsenceAgents> ?? target.ToList();
			foreach (var payloadWorkTime in absenceTime)
			{
				var absenceAgent = absenceAgentses.FirstOrDefault(a => a.Date == payloadWorkTime.BelongsToDate);
				if (absenceAgent == null)
				{
					hasMismatch = true;
					continue;
				}
				absenceAgent.AbsenceTime += TimeSpan.FromTicks(payloadWorkTime.TotalContractTime).TotalMinutes;
				absenceAgent.HeadCounts += payloadWorkTime.HeadCounts;
			}
			return hasMismatch;
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
			return _scheduleProjectionReadOnlyPersister.AbsenceTimePerBudgetGroup(period, budgetGroup, scenario);
		}	
	}
}



