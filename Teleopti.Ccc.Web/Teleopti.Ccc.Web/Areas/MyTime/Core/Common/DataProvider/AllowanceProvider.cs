using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AllowanceProvider : IAllowanceProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		public readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

		public AllowanceProvider(IBudgetDayRepository budgetDayRepository, IUserTimeZone userTimeZone, ILoggedOnUser loggedOnUser, IScenarioRepository scenarioRepository)
		{
			_budgetDayRepository = budgetDayRepository;
			_userTimeZone = userTimeZone;
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_scheduleProjectionReadOnlyRepository = new ScheduleProjectionReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
		}

		public IEnumerable<IAllowanceDay> GetAllowanceForPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var allowanceList = new List<IAllowanceDay>();
			IBudgetGroup budgetGroup;
			try
			{
				budgetGroup = person.PersonPeriodCollection.Last().BudgetGroup;
			}
			catch (Exception)
			{
				foreach (var day in period.DayCollection())
				{
					var allowanceDay = new AllowanceDay { Allowance = 0, Date = day };
					allowanceList.Add(allowanceDay);
				}

				return allowanceList;
			}
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();

			var budgetDays = _budgetDayRepository.Find(defaultScenario, budgetGroup, period);

			foreach (var day in period.DayCollection())
			{
				var allowanceDay = new AllowanceDay();
				allowanceDay.Allowance = 0;
				allowanceDay.Date = day;

				foreach (var budgetDay in budgetDays)
				{
					if (budgetDay.Day != day) continue;
					allowanceDay.Allowance = budgetDay.Allowance;
					allowanceDay.Date = day;
				}
				allowanceList.Add(allowanceDay);
			}
			return allowanceList;
		}
	}

	public class AllowanceDay : IAllowanceDay
	{
		public DateTime Date { get; set; }
		public double Allowance { get; set; }
	}
}