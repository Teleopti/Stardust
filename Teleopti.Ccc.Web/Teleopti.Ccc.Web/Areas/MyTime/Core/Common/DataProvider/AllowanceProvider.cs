using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public AllowanceProvider(IBudgetDayRepository budgetDayRepository, ILoggedOnUser loggedOnUser,
			IScenarioRepository scenarioRepository, IExtractBudgetGroupPeriods extractBudgetGroupPeriods, INow now)
		{
			_budgetDayRepository = budgetDayRepository;
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_extractBudgetGroupPeriods = extractBudgetGroupPeriods;
			_now = now;
		}

		public IEnumerable<IAllowanceDay> GetAllowanceForPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var userTimezone = person.PermissionInformation.DefaultTimeZone();

			var allowanceList =
				from d in period.DayCollection()
				select new
				{
					Date = d,
					Time = TimeSpan.Zero,
					Heads = TimeSpan.Zero,
					AllowanceHeads = .0,
					Availability = false,
					UseHeadCount = false,
					ValidateBudgetGroup = false,
				};

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

				if (validOpenPeriods.Count > 0)
				{
					allowanceList =
						from d in period.DayCollection()
						select new
						{
							Date = d,
							Time = TimeSpan.Zero,
							Heads = TimeSpan.Zero,
							AllowanceHeads = .0,
							Availability = true,
							UseHeadCount = false,
							ValidateBudgetGroup = false
						};

					var budgetDays =
						budgetGroupPeriods.SelectMany(x => _budgetDayRepository.Find(defaultScenario, x.Item2, x.Item1)).ToList();
					foreach (var thePeriod in validOpenPeriods)
					{
						var openPeriod = thePeriod;
						var allowanceFromBudgetDays =
							from budgetDay in budgetDays
							where openPeriod.GetPeriod(new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), userTimezone))).Contains(budgetDay.Day)
							where openPeriod.OpenForRequestsPeriod.Contains(new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), userTimezone)))
							where openPeriod.AbsenceRequestProcess.GetType() != typeof(DenyAbsenceRequest)
							select
								new
								{
									Date = budgetDay.Day,
									Time = TimeSpan.FromHours(Math.Max(budgetDay.ShrinkedAllowance * budgetDay.FulltimeEquivalentHours, 0)),
									Heads = TimeSpan.FromHours(Math.Max(budgetDay.FulltimeEquivalentHours, 0)),
									AllowanceHeads = budgetDay.ShrinkedAllowance,
									Availability = true,
									UseHeadCount = openPeriod.StaffingThresholdValidator.GetType() == typeof(BudgetGroupHeadCountValidator),
									ValidateBudgetGroup = true
								};
						allowanceList = allowanceList.Concat(allowanceFromBudgetDays);
					}
				}
			}

			return
				from p in allowanceList
				group p by p.Date
				into g
				orderby g.Key
				select new AllowanceDay
				{
					Date = g.Key,
					Time = g.Last(o => o.Date == g.Key).Time,
					Heads = g.Last().Heads,
					AllowanceHeads = g.Last(o => o.Date == g.Key).AllowanceHeads,
					Availability = g.Last(o => o.Date == g.Key).Availability,
					UseHeadCount = g.Last(o => o.Date == g.Key).UseHeadCount,
					ValidateBudgetGroup = g.Any(o => o.ValidateBudgetGroup)
				};
		}
	}
}