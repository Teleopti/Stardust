using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;


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
			var userToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), userTimezone));

			var periodsOpenedForToday = person.WorkflowControlSet?.AbsenceRequestOpenPeriods?
				.Where(p => p.OpenForRequestsPeriod.Contains(userToday)).ToList();
			if (periodsOpenedForToday == null || !periodsOpenedForToday.Any())
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

			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var budgetDays = _extractBudgetGroupPeriods.BudgetGroupsForPeriod(person, period)
				.SelectMany(x => _budgetDayRepository.Find(defaultScenario, x.Item2, x.Item1)).ToList();

			return createAllowanceDays(period, userToday, periodsOpenedForToday, budgetDays);
		}

		private static IEnumerable<IAllowanceDay> createAllowanceDays(DateOnlyPeriod period, DateOnly userToday,
			IEnumerable<IAbsenceRequestOpenPeriod> openPeriods, IReadOnlyCollection<IBudgetDay> budgetDays)
		{
			var result = new List<IAllowanceDay>();
			var sortedOpenPeriods = openPeriods.OrderByDescending(p => p.OrderIndex).ToList();
			foreach (var date in period.DayCollection())
			{
				var openPeriodsForThisDay = sortedOpenPeriods.FirstOrDefault(p => p.GetPeriod(userToday).Contains(date));
				var openPeriodIsValid = openPeriodsForThisDay != null && isValidOpenPeriod(openPeriodsForThisDay);
				var budgetDayForThisDay = budgetDays?.FirstOrDefault(bd => bd.Day == date);

				if (!openPeriodIsValid || budgetDayForThisDay == null)
				{
					result.Add(new AllowanceDay
					{
						Date = date,
						Time = TimeSpan.Zero,
						Heads = TimeSpan.Zero,
						Availability = openPeriodIsValid,
						AllowanceHeads = .0,
						UseHeadCount = false,
						ValidateBudgetGroup = false
					});
					continue;
				}

				var useHeadCount = openPeriodsForThisDay.StaffingThresholdValidator is BudgetGroupHeadCountValidator;
				var shrinkedAllowance = budgetDayForThisDay.ShrinkedAllowance;
				var fullTimeInHours = budgetDayForThisDay.FulltimeEquivalentHours;
				result.Add(new AllowanceDay
				{
					Date = date,
					Time = TimeSpan.FromHours(Math.Max(shrinkedAllowance * fullTimeInHours, 0)),
					Heads = TimeSpan.FromHours(Math.Max(fullTimeInHours, 0)),
					Availability = true,
					AllowanceHeads = shrinkedAllowance,
					UseHeadCount = useHeadCount,
					ValidateBudgetGroup = true
				});
			}

			return result;
		}

		private static bool isValidOpenPeriod(IAbsenceRequestOpenPeriod openPeriod)
		{
			var validateStaffingWithBudgetGroup = openPeriod.StaffingThresholdValidator is BudgetGroupAllowanceValidator ||
												  openPeriod.StaffingThresholdValidator is BudgetGroupHeadCountValidator;
			var isAutoDeny = openPeriod.AbsenceRequestProcess is DenyAbsenceRequest;
			return validateStaffingWithBudgetGroup && !isAutoDeny;
		}
	}
}
