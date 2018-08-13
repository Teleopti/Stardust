using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public class BudgetGroupHeadCountSpecificationLimited : IBudgetGroupHeadCountCalculator
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;

		public BudgetGroupHeadCountSpecificationLimited(IScenarioRepository scenarioRepository, IBudgetDayRepository budgetDayRepository, IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister)
		{
			_scenarioRepository = scenarioRepository;
			_budgetDayRepository = budgetDayRepository;
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
		}

		public IValidatedRequest IsSatisfied(IAbsenceRequestAndSchedules absenceRequestSchedules)
		{
			var timeZone = absenceRequestSchedules.AbsenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var culture = absenceRequestSchedules.AbsenceRequest.Person.PermissionInformation.Culture();
			var language = absenceRequestSchedules.AbsenceRequest.Person.PermissionInformation.UICulture();
			var requestedPeriod = absenceRequestSchedules.AbsenceRequest.Period.ToDateOnlyPeriod(timeZone);
			var personPeriod = absenceRequestSchedules.AbsenceRequest.Person.Period(requestedPeriod.StartDate);

			if (personPeriod?.BudgetGroup == null)
			{
				return AbsenceRequestBudgetGroupValidationHelper.PersonPeriodOrBudgetGroupIsNull(culture, absenceRequestSchedules.AbsenceRequest.Person.Id);
			}

			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var budgetDays = _budgetDayRepository.Find(defaultScenario, personPeriod.BudgetGroup, requestedPeriod);
			if (budgetDays == null)
			{
				return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNull(language, culture, requestedPeriod);
			}

			if (budgetDays.Count != requestedPeriod.DayCount())
			{
				return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNotEqualToRequestedPeriodDays(language, culture, requestedPeriod);
			}

			var invalidDays = getInvalidDaysIfExist(budgetDays, personPeriod.BudgetGroup, culture, absenceRequestSchedules.SchedulingResultStateHolder);
			if (!string.IsNullOrEmpty(invalidDays))
			{
				var notEnoughAllowance = Resources.ResourceManager.GetString(nameof(Resources.NotEnoughBudgetAllowanceForTheDay), language);
				return AbsenceRequestBudgetGroupValidationHelper.InvalidDaysInBudgetDays(invalidDays, notEnoughAllowance);
			}

			return new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty };
		}

		private string getInvalidDaysIfExist(IEnumerable<IBudgetDay> budgetDays, IBudgetGroup budgetGroup, CultureInfo culture, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var count = 0;
			var invalidDays = string.Empty;
			var alreayAddedHeadCountDictionary = new Dictionary<IBudgetDay, int>();

			foreach (var budgetDay in budgetDays.OrderBy(x => x.Day))
			{
				var currentDay = budgetDay.Day;
				var allowance = budgetDay.ShrinkedAllowance;
				var alreadyUsedAllowance = _scheduleProjectionReadOnlyPersister.GetNumberOfAbsencesPerDayAndBudgetGroup(
					budgetGroup.Id.GetValueOrDefault(), currentDay);
				var addedBefore = schedulingResultStateHolder.AddedAbsenceHeadCountDuringCurrentRequestHandlingCycle(budgetDay);
				alreadyUsedAllowance += addedBefore;
				if (Math.Floor(allowance) <= alreadyUsedAllowance)
				{
					if (alreayAddedHeadCountDictionary.Any())
					{
						alreayAddedHeadCountDictionary.ForEach(item =>
						{
							schedulingResultStateHolder
								.SubtractAbsenceHeadCountDuringCurrentRequestHandlingCycle(item.Key);
						});
					}

					count++;
					if (count > 5) break;
					invalidDays += currentDay.ToShortDateString(culture) + ",";
				}
				else
				{
					schedulingResultStateHolder.AddAbsenceHeadCountDuringCurrentRequestHandlingCycle(budgetDay);
					alreayAddedHeadCountDictionary.Add(budgetDay, 1);
				}
			}
			return invalidDays.IsEmpty() ? invalidDays : invalidDays.Substring(0, invalidDays.Length - 1);
		}
	}
}