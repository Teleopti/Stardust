using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Sdk.ServiceBus.AbsenceRequest;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "HeadCount")]
	public class BudgetGroupHeadCountSpecification : PersonRequestSpecification<IAbsenceRequest>, IBudgetGroupHeadCountSpecification
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

		public BudgetGroupHeadCountSpecification(IScenarioRepository scenarioRepository, IBudgetDayRepository budgetDayRepository, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
		{
			_scenarioRepository = scenarioRepository;
			_budgetDayRepository = budgetDayRepository;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		}

		public override IValidatedRequest IsSatisfied(IAbsenceRequest absenceRequest)
		{
			var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var culture = absenceRequest.Person.PermissionInformation.Culture();
			var requestedPeriod = absenceRequest.Period.ToDateOnlyPeriod(timeZone);
			var personPeriod = absenceRequest.Person.PersonPeriods(requestedPeriod).FirstOrDefault();

			if (personPeriod == null || personPeriod.BudgetGroup == null)
			{
				return AbsenceRequestBudgetGroupValidationHelper.PersonPeriodOrBudgetGroupIsNull(culture, absenceRequest.Person.Id);
			}

			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var budgetDays = _budgetDayRepository.Find(defaultScenario, personPeriod.BudgetGroup, requestedPeriod);
			if (budgetDays == null)
			{
				return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNull(culture, requestedPeriod);
			}

			if (budgetDays.Count != requestedPeriod.DayCollection().Count)
			{
				return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNotEqualToRequestedPeriodDays(culture, requestedPeriod);
			}

			var invalidDays = getInvalidDaysIfExist(budgetDays, personPeriod.BudgetGroup, culture);
			if (!string.IsNullOrEmpty(invalidDays))
			{
				var notEnoughAllowance = UserTexts.Resources.ResourceManager.GetString("NotEnoughAllowance", culture);
				return AbsenceRequestBudgetGroupValidationHelper.InvalidDaysInBudgetDays(invalidDays, notEnoughAllowance);
			}

			return new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty };
		}

		private string getInvalidDaysIfExist(IEnumerable<IBudgetDay> budgetDays, IBudgetGroup budgetGroup, CultureInfo culture)
		{
			var count = 0;
			var invalidDays = string.Empty;

			foreach (var budgetDay in budgetDays.OrderBy(x => x.Day))
			{
				if (budgetDay.IsClosed) continue;

				var currentDay = budgetDay.Day;
				var allowance = budgetDay.Allowance;
				var alreadyUsedAllowance = _scheduleProjectionReadOnlyRepository.GetNumberOfAbsencesPerDayAndBudgetGroup(
						budgetGroup.Id.GetValueOrDefault(), currentDay);

				if (Math.Floor(allowance) <= alreadyUsedAllowance)
				{
					count++;
					if (count > 5) break;
					invalidDays += currentDay.ToShortDateString(culture) + ",";
				}
			}
			return invalidDays.IsEmpty() ? invalidDays : invalidDays.Substring(0, invalidDays.Length - 1);
		}
	}
}
