using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AllowanceProvider : IAllowanceProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		public readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IExtractBudgetGroupPeriods _extractBudgetGroupPeriods;

		public AllowanceProvider(IBudgetDayRepository budgetDayRepository, ILoggedOnUser loggedOnUser, IScenarioRepository scenarioRepository, IExtractBudgetGroupPeriods extractBudgetGroupPeriods)
		{
			_budgetDayRepository = budgetDayRepository;
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_extractBudgetGroupPeriods = extractBudgetGroupPeriods;
		}

		public IEnumerable<IAllowanceDay> GetAllowanceForPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var allowanceList = period.DayCollection().Select(dateOnly => new AllowanceDay
				                                                              {
					                                                              Allowance = 0d, Date = dateOnly
				                                                              }).ToList();

			var budgetGroupPeriods = _extractBudgetGroupPeriods.BudgetGroupsForPeriod(person, period);
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();

			foreach (var budgetGroupPeriod in budgetGroupPeriods)
			{
				var budgetDays = _budgetDayRepository.Find(defaultScenario, budgetGroupPeriod.Item2, budgetGroupPeriod.Item1);

				foreach (var budgetDay in budgetDays)
				{
					var allowanceDay = allowanceList.FirstOrDefault(a => a.Date == budgetDay.Day);
					if (allowanceDay != null)
					{
						allowanceDay.Allowance = budgetDay.Allowance * budgetDay.FulltimeEquivalentHours * 60;
					}
				}
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