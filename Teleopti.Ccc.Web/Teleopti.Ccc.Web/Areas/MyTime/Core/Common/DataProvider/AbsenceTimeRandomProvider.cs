using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AbsenceTimeRandomProvider : IAbsenceTimeRandomProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		public readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

		public AbsenceTimeRandomProvider(IBudgetDayRepository budgetDayRepository, IUserTimeZone userTimeZone, ILoggedOnUser loggedOnUser, IScenarioRepository scenarioRepository, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
		{
			_budgetDayRepository = budgetDayRepository;
			_userTimeZone = userTimeZone;
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

			var random = new Random();
			return period.DayCollection().Select(day => new AbsenceAgents() { Date = day, AbsenceTime = random.Next(1,23) }).Cast<IAbsenceAgents>().ToList();
		}
	}

	public class AbsenceAgents : IAbsenceAgents
	{
		public DateTime Date { get; set; }
		public double AbsenceTime { get; set; }
	}
}