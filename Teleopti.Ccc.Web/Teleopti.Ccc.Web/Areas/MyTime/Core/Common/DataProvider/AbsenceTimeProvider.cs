using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
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

		public AbsenceTimeProvider(ILoggedOnUser loggedOnUser, IScenarioRepository scenarioRepository, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository,IExtractBudgetGroupPeriods extractBudgetGroupPeriod)
		{
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
			_extractBudgetGroupPeriod = extractBudgetGroupPeriod;
		}

		private void AddTime(IEnumerable<AbsenceAgents> target, DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			if (budgetGroup != null)
			{
				var absenceTime = _scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(period, budgetGroup, scenario);

				if (absenceTime != null)
				{
					foreach (var payloadWorkTime in absenceTime)
					{
						target.First(a => a.Date == payloadWorkTime.BelongsToDate).AbsenceTime = TimeSpan.FromTicks(payloadWorkTime.TotalContractTime).TotalMinutes;
					}
				}
			}
		}
	
		public IEnumerable<IAbsenceAgents> GetAbsenceTimeForPeriod(DateOnlyPeriod period)
		{
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			List<AbsenceAgents> absenceDays = period.DayCollection().Select(d => new AbsenceAgents() { Date = d, AbsenceTime = 0 }).ToList();

			var budgetGroupsPeriod = _extractBudgetGroupPeriod.BudgetGroupsForPeriod(_loggedOnUser.CurrentUser(), period);
			foreach (var tuple in budgetGroupsPeriod)
			{
				AddTime(absenceDays, tuple.Item1, tuple.Item2, defaultScenario);
			}

			return absenceDays;
		}
	}
}