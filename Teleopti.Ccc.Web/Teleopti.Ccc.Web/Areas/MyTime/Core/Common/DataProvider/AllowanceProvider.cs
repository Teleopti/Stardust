using System;
using System.Collections.Generic;
using System.Linq;
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
			if (person.WorkflowControlSet?.AbsenceRequestOpenPeriods == null)
			{
				return createEmptyAllowanceDayCollection(period);
			}

			var userTimezone = person.PermissionInformation.DefaultTimeZone();
			var userToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), userTimezone));

			var absenceRequestOpenPeriods = person.WorkflowControlSet.AbsenceRequestOpenPeriods;
			var validOpenPeriods = absenceRequestOpenPeriods
				.Where(p => isValidOpenPeriod(p) && p.OpenForRequestsPeriod.Contains(userToday))
				.OrderBy(p => p.StaffingThresholdValidatorList.IndexOf(p.StaffingThresholdValidator));
			if (!validOpenPeriods.Any())
			{
				return createEmptyAllowanceDayCollection(period);
			}

			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var budgetGroupPeriods = _extractBudgetGroupPeriods.BudgetGroupsForPeriod(person, period);
			var budgetDays = budgetGroupPeriods
				.SelectMany(x => _budgetDayRepository.Find(defaultScenario, x.Item2, x.Item1)).ToList();

			var allowanceList = from date in period.DayCollection()
				select new
				{
					Date = date,
					Time = TimeSpan.Zero,
					Heads = TimeSpan.Zero,
					AllowanceHeads = .0,
					UseHeadCount = false,
					ValidateBudgetGroup = false,
				};

			foreach (var openPeriod in validOpenPeriods)
			{
				var periodForToday = openPeriod.GetPeriod(userToday);
				var useHeadCount = openPeriod.StaffingThresholdValidator is BudgetGroupHeadCountValidator;
				var allowanceFromBudgetDays =
					from budgetDay in budgetDays
					where periodForToday.Contains(budgetDay.Day)
					let shrinkedAllowance = budgetDay.ShrinkedAllowance
					let fulltimeInHours = budgetDay.FulltimeEquivalentHours
					select new
					{
						Date = budgetDay.Day,
						Time = TimeSpan.FromHours(Math.Max(shrinkedAllowance * fulltimeInHours, 0)),
						Heads = TimeSpan.FromHours(Math.Max(fulltimeInHours, 0)),
						AllowanceHeads = shrinkedAllowance,
						UseHeadCount = useHeadCount,
						ValidateBudgetGroup = true
					};
				allowanceList = allowanceList.Concat(allowanceFromBudgetDays);
			}

			return from rawData in allowanceList
				group rawData by rawData.Date
				into g
				orderby g.Key
				select new AllowanceDay
				{
					Date = g.Key,
					Time = g.Last(o => o.Date == g.Key).Time,
					Heads = g.Last().Heads,
					AllowanceHeads = g.Last(o => o.Date == g.Key).AllowanceHeads,
					Availability = true,
					UseHeadCount = g.Last(o => o.Date == g.Key).UseHeadCount,
					ValidateBudgetGroup = g.Any(o => o.ValidateBudgetGroup)
				};
		}

		private static IEnumerable<AllowanceDay> createEmptyAllowanceDayCollection(DateOnlyPeriod period)
		{
			return period.DayCollection().Select(date => new AllowanceDay
			{
				Date = date,
				Time = TimeSpan.Zero,
				Heads = TimeSpan.Zero,
				AllowanceHeads = .0,
				Availability = false,
				UseHeadCount = false,
				ValidateBudgetGroup = false
			});
		}

		private static bool isValidOpenPeriod(IAbsenceRequestOpenPeriod period)
		{
			var isValidateByBudgetGroup = period.StaffingThresholdValidator is BudgetGroupAllowanceValidator ||
										period.StaffingThresholdValidator is BudgetGroupHeadCountValidator;
			var isAutoDeny = period.AbsenceRequestProcess is DenyAbsenceRequest;
			return isValidateByBudgetGroup && !isAutoDeny;
		}
	}
}
