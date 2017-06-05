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
			var validOpenPeriods = person.WorkflowControlSet?.AbsenceRequestOpenPeriods?
				.Where(isValidateStaffingWithBudgetGroup).ToList();
			if (validOpenPeriods == null || !validOpenPeriods.Any())
			{
				return createEmptyAllowanceDayCollection(period, false);
			}

			var userTimezone = person.PermissionInformation.DefaultTimeZone();
			var userToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), userTimezone));
			validOpenPeriods = validOpenPeriods.Where(p => isValidOpenPeriod(p, userToday))
				.OrderBy(p => p.OrderIndex).ToList();

			if (!validOpenPeriods.Any())
			{
				return createEmptyAllowanceDayCollection(period, true);
			}

			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var budgetDays = _extractBudgetGroupPeriods.BudgetGroupsForPeriod(person, period)
				.SelectMany(x => _budgetDayRepository.Find(defaultScenario, x.Item2, x.Item1)).ToList();

			return budgetDays.Any()
				? createAllowanceDays(period, userToday, validOpenPeriods, budgetDays)
				: createEmptyAllowanceDayCollection(period, true);
		}

		private static bool isValidateStaffingWithBudgetGroup(IAbsenceRequestOpenPeriod period)
		{
			return period.StaffingThresholdValidator is BudgetGroupAllowanceValidator ||
					period.StaffingThresholdValidator is BudgetGroupHeadCountValidator;
		}

		private static bool isValidOpenPeriod(IAbsenceRequestOpenPeriod period, DateOnly userToday)
		{
			var isAutoDeny = period.AbsenceRequestProcess is DenyAbsenceRequest;
			return !isAutoDeny && period.OpenForRequestsPeriod.Contains(userToday);
		}

		private static IEnumerable<AllowanceDay> createEmptyAllowanceDayCollection(DateOnlyPeriod period, bool availability)
		{
			return period.DayCollection().Select(date => new AllowanceDay
			{
				Date = date,
				Time = TimeSpan.Zero,
				Heads = TimeSpan.Zero,
				AllowanceHeads = .0,
				Availability = availability,
				UseHeadCount = false,
				ValidateBudgetGroup = false
			});
		}

		private static IEnumerable<IAllowanceDay> createAllowanceDays(DateOnlyPeriod period, DateOnly userToday,
			IEnumerable<IAbsenceRequestOpenPeriod> validOpenPeriods, IReadOnlyCollection<IBudgetDay> budgetDays)
		{
			var allowanceList = from d in period.DayCollection()
				select new
				{
					Date = d,
					Time = TimeSpan.Zero,
					Heads = TimeSpan.Zero,
					AllowanceHeads = .0,
					UseHeadCount = false,
					ValidateBudgetGroup = false
				};

			foreach (var openPeriod in validOpenPeriods)
			{
				var periodForToday = openPeriod.GetPeriod(userToday);
				var useHeadCount = openPeriod.StaffingThresholdValidator is BudgetGroupHeadCountValidator;
				var validBudgetDays = budgetDays.Where(bd => periodForToday.Contains(bd.Day));
				var allowanceFromBudgetDays =
					from budgetDay in validBudgetDays
					let shrinkedAllowance = budgetDay.ShrinkedAllowance
					let fullTimeInHours = budgetDay.FulltimeEquivalentHours
					select new
					{
						Date = budgetDay.Day,
						Time = TimeSpan.FromHours(Math.Max(shrinkedAllowance * fullTimeInHours, 0)),
						Heads = TimeSpan.FromHours(Math.Max(fullTimeInHours, 0)),
						AllowanceHeads = shrinkedAllowance,
						UseHeadCount = useHeadCount,
						ValidateBudgetGroup = true
					};
				allowanceList = allowanceList.Concat(allowanceFromBudgetDays);
			}

			return from rawAllowance in allowanceList
				group rawAllowance by rawAllowance.Date
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
	}
}
