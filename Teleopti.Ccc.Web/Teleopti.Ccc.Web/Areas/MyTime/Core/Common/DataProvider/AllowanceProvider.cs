using System;
using System.Collections.Generic;
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
		private INow _now;

		public AllowanceProvider(IBudgetDayRepository budgetDayRepository, ILoggedOnUser loggedOnUser, IScenarioRepository scenarioRepository, IExtractBudgetGroupPeriods extractBudgetGroupPeriods, INow now)
		{
			_budgetDayRepository = budgetDayRepository;
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_extractBudgetGroupPeriods = extractBudgetGroupPeriods;
			_now = now;
		}

		public IEnumerable<Tuple<DateOnly, TimeSpan>> GetAllowanceForPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();

			var allowanceList =
				from d in period.DayCollection()
				select new { Date = d, Time = TimeSpan.Zero };

			var budgetGroupPeriods = _extractBudgetGroupPeriods.BudgetGroupsForPeriod(person, period);
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();

			if (person.WorkflowControlSet != null && person.WorkflowControlSet.AbsenceRequestOpenPeriods != null)
			{

				var openPeriods = person.WorkflowControlSet.AbsenceRequestOpenPeriods;

				if (openPeriods.Any(o => o.OpenForRequestsPeriod.Contains(_now.DateOnly())))
				{

					var allowanceFromBudgetDays =
						from budgetGroupPeriod in budgetGroupPeriods
						from budgetDay in _budgetDayRepository.Find(defaultScenario, budgetGroupPeriod.Item2, budgetGroupPeriod.Item1)
						where openPeriods.Any(o => o.GetPeriod(budgetDay.Day).Contains(budgetDay.Day)) 
						select
							new
								{
									Date = budgetDay.Day,
									Time = TimeSpan.FromHours(Math.Max(budgetDay.Allowance*budgetDay.FulltimeEquivalentHours, 0))
								};

					allowanceList = allowanceList.Concat(allowanceFromBudgetDays);
				}
			}

			return
			from p in allowanceList
			group p by p.Date into g
			orderby g.Key
			select new Tuple<DateOnly, TimeSpan>(g.Key, TimeSpan.FromTicks(g.Sum(p => p.Time.Ticks)));

		}
	}
}