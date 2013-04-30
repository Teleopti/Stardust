using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AllowanceProvider : IAllowanceProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IExtractBudgetGroupPeriods _extractBudgetGroupPeriods;

		public AllowanceProvider(IBudgetDayRepository budgetDayRepository, ILoggedOnUser loggedOnUser, IScenarioRepository scenarioRepository, IExtractBudgetGroupPeriods extractBudgetGroupPeriods)
		{
			_budgetDayRepository = budgetDayRepository;
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_extractBudgetGroupPeriods = extractBudgetGroupPeriods;
		}

		public IEnumerable<Tuple<DateOnly, TimeSpan>> GetAllowanceForPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var allowanceList = period.DayCollection().Select(dateOnly => new Tuple<DateOnly, TimeSpan>(dateOnly,TimeSpan.Zero)).ToList();

			var budgetGroupPeriods = _extractBudgetGroupPeriods.BudgetGroupsForPeriod(person, period);
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();

			ReadOnlyCollection<IAbsenceRequestOpenPeriod> openPeriods;
			if (person.WorkflowControlSet != null && person.WorkflowControlSet.AbsenceRequestOpenPeriods != null)
			{
				openPeriods = person.WorkflowControlSet.AbsenceRequestOpenPeriods;
			}
			else
			{
				return allowanceList;
			}

			foreach (var budgetGroupPeriod in budgetGroupPeriods)
			{
				var budgetDays = _budgetDayRepository.Find(defaultScenario, budgetGroupPeriod.Item2, budgetGroupPeriod.Item1);

				if (person.WorkflowControlSet != null)
				foreach (var budgetDay in budgetDays)
				{
					foreach (var openPeriod in openPeriods)
					{
						if (!openPeriod.OpenForRequestsPeriod.Contains(budgetDay.Day)) continue;

						var allowanceDay = allowanceList.FirstOrDefault(a => a.Item1 == budgetDay.Day);
						if (allowanceDay != null)
						{
							var index = allowanceList.IndexOf(allowanceDay); 
							allowanceList.Insert(index,new Tuple<DateOnly, TimeSpan>(allowanceDay.Item1, TimeSpan.FromHours(budgetDay.Allowance * budgetDay.FulltimeEquivalentHours)));
							allowanceList.Remove(allowanceDay);

						}
					}
				}
			}

			return allowanceList;
		}
	}
}