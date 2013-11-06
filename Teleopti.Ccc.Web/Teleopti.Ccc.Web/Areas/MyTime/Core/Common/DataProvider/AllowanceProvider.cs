using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AllowanceProvider : IAllowanceProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IExtractBudgetGroupPeriods _extractBudgetGroupPeriods;
		private readonly INow _now;

		public AllowanceProvider(IBudgetDayRepository budgetDayRepository, ILoggedOnUser loggedOnUser, IScenarioRepository scenarioRepository, IExtractBudgetGroupPeriods extractBudgetGroupPeriods, INow now)
		{
			_budgetDayRepository = budgetDayRepository;
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_extractBudgetGroupPeriods = extractBudgetGroupPeriods;
			_now = now;
		}

		public IEnumerable<Tuple<DateOnly, TimeSpan, TimeSpan, double, bool, bool>> GetAllowanceForPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();

			var allowanceList =
				from d in period.DayCollection()
				select new { Date = d, Time = TimeSpan.Zero, Heads = TimeSpan.Zero, Allowance = .0, Availability = false, UseHeadCount = false};

			var budgetGroupPeriods = _extractBudgetGroupPeriods.BudgetGroupsForPeriod(person, period);
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();

			if (person.WorkflowControlSet != null && person.WorkflowControlSet.AbsenceRequestOpenPeriods != null)
			{

				var openPeriods = person.WorkflowControlSet.AbsenceRequestOpenPeriods;

				var unSorted = openPeriods.Where(
					absenceRequestOpenPeriod => 
					absenceRequestOpenPeriod.StaffingThresholdValidator.GetType() == typeof(BudgetGroupAllowanceValidator) ||
					absenceRequestOpenPeriod.StaffingThresholdValidator.GetType() == typeof(BudgetGroupHeadCountValidator)).ToList();

				var validOpenPeriods = (from p in unSorted
							 orderby p.StaffingThresholdValidatorList.IndexOf(p.StaffingThresholdValidator)
							 select p).ToList();

				var invalidOpenPeriods = openPeriods.Where(
					absenceRequestOpenPeriod =>
					absenceRequestOpenPeriod.StaffingThresholdValidator.GetType() != typeof(BudgetGroupAllowanceValidator) &&
					absenceRequestOpenPeriod.StaffingThresholdValidator.GetType() != typeof(BudgetGroupHeadCountValidator)).ToList();

				if (validOpenPeriods.Count != 0)
				{
					allowanceList =
						from d in period.DayCollection()
						select new
							{
								Date = d,
								Time = TimeSpan.Zero,
								Heads = TimeSpan.Zero,
								Allowance = .0,
								Availability = (invalidOpenPeriods.Count <= 0 || !invalidOpenPeriods.Any(x => x.GetPeriod(_now.LocalDateOnly()).Contains(d))),
								UseHeadCount = false
							};


					foreach (var thePeriod in validOpenPeriods)
					{
						var openPeriod = thePeriod;
						var allowanceFromBudgetDays =
							from budgetGroupPeriod in budgetGroupPeriods
							from budgetDay in _budgetDayRepository.Find(defaultScenario, budgetGroupPeriod.Item2, budgetGroupPeriod.Item1)
							where openPeriod.GetPeriod(_now.LocalDateOnly()).Contains(budgetDay.Day)
							where openPeriod.OpenForRequestsPeriod.Contains(_now.LocalDateOnly())
							where openPeriod.AbsenceRequestProcess.GetType() != typeof (DenyAbsenceRequest)
							select
								new
									{
										Date = budgetDay.Day,
										Time = TimeSpan.FromHours(Math.Max(budgetDay.Allowance*budgetDay.FulltimeEquivalentHours, 0)),
										Heads = TimeSpan.FromHours(Math.Max(budgetDay.FulltimeEquivalentHours, 0)),
										Allowance = budgetDay.Allowance,
										Availability = (invalidOpenPeriods.Count <= 0 || !invalidOpenPeriods.Any(x => x.GetPeriod(_now.LocalDateOnly()).Contains(budgetDay.Day))),
										UseHeadCount = openPeriod.StaffingThresholdValidator.GetType() == typeof(BudgetGroupHeadCountValidator)
									};
						allowanceList = allowanceList.Concat(allowanceFromBudgetDays);
					}
				}
			}

			return 
				from p in allowanceList
				group p by p.Date into g
				orderby g.Key
				select new Tuple<DateOnly, TimeSpan, TimeSpan, double, bool, bool>
                    (g.Key, TimeSpan.FromTicks(g.Last(o => o.Date == g.Key).Time.Ticks), TimeSpan.FromTicks(g.Last().Heads.Ticks), g.Last(o => o.Date == g.Key).Allowance,
					g.Last(o => o.Date == g.Key).Availability, g.Last(o => o.Date == g.Key).UseHeadCount);
		}

		

	}
}