using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
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

		public AbsenceTimeProvider(ILoggedOnUser loggedOnUser, IScenarioRepository scenarioRepository, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, IExtractBudgetGroupPeriods extractBudgetGroupPeriod)
		{
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
			_extractBudgetGroupPeriod = extractBudgetGroupPeriod;
		}

		public IEnumerable<IAbsenceAgents> GetAbsenceTimeForPeriod(DateOnlyPeriod period)
		{
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			List<AbsenceAgents> absenceDays = period.DayCollection().Select(d => new AbsenceAgents { Date = d.Date, AbsenceTime = 0, HeadCounts = 0 }).ToList();

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

			fillAbsenceTimeInformationOnAbsenceAgents(target, getAbsenceTime (period, budgetGroup, scenario));
		}

		private static void fillAbsenceTimeInformationOnAbsenceAgents (IEnumerable<AbsenceAgents> target, IEnumerable<PayloadWorkTime> absenceTime)
		{
			if (absenceTime == null) return;

			var absenceAgentses = target as IList<AbsenceAgents> ?? target.ToList();
			foreach (var payloadWorkTime in absenceTime)
			{
				var absenceAgent = absenceAgentses.First (a => a.Date == payloadWorkTime.BelongsToDate);
				absenceAgent.AbsenceTime += TimeSpan.FromTicks (payloadWorkTime.TotalContractTime).TotalMinutes;
				absenceAgent.HeadCounts += payloadWorkTime.HeadCounts;
			}
		}

		private IEnumerable<PayloadWorkTime> getAbsenceTime(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			var cacheKey = GetCacheKey(period, budgetGroup, scenario);
			return getAbsenceTimeFromCache(cacheKey) ?? getAbsenceTimeFromRepository (period, budgetGroup, scenario, cacheKey);
		}

		private static IEnumerable<PayloadWorkTime> getAbsenceTimeFromCache (string cacheKey)
		{
			return HttpRuntime.Cache.Get (cacheKey) as IEnumerable<PayloadWorkTime>;
		}

		private IEnumerable<PayloadWorkTime> getAbsenceTimeFromRepository (DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario, string cacheKey)
		{
			var absenceTime = _scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup (period, budgetGroup, scenario);
			addAbsenceTimeToCache (cacheKey, absenceTime);

			return absenceTime;
		}

		private static void addAbsenceTimeToCache(String cacheKey, IEnumerable<PayloadWorkTime> absenceTime)
		{
			HttpRuntime.Cache.Add (cacheKey, absenceTime, null, Cache.NoAbsoluteExpiration, 
				new TimeSpan (0, 0, 5, 0), CacheItemPriority.Normal, null);
		}

		public static String GetCacheKey(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			return period.DateString + budgetGroup.Id + scenario.Id;
		}
	}
}