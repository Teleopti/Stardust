using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AbsenceTimeProvider : IAbsenceTimeProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

		public AbsenceTimeProvider(IBudgetDayRepository budgetDayRepository, ILoggedOnUser loggedOnUser, IScenarioRepository scenarioRepository, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
		{
			_budgetDayRepository = budgetDayRepository;
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		}

		public IEnumerable<IAbsenceAgents> GetAbsenceTimeForPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var budgetGroup = person.PersonPeriodCollection.Last().BudgetGroup;
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();

			var budgetDays = _budgetDayRepository.Find(defaultScenario, budgetGroup, period);
			var list = new List<double>();

			foreach (var budgetDay in budgetDays.OrderBy(x => x.Day))
			{
				var currentDay = budgetDay.Day;
				var x = _scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(currentDay, currentDay),
				                                                                        budgetGroup, defaultScenario);
				double usedAbsenceMinutes = 0;
				if (x != null)
				{
					usedAbsenceMinutes = TimeSpan.FromTicks(
						x.Sum(p => p.TotalContractTime)).TotalMinutes;
				}
				list.Add(usedAbsenceMinutes);
			}

			return period.DayCollection().Select(day => new AbsenceAgents() { Date = day, AbsenceTime = list.Sum()}).Cast<IAbsenceAgents>().ToList();
		}
	}
}