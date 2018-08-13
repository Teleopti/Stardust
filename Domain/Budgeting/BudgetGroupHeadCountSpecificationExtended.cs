using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public class BudgetGroupHeadCountSpecificationExtended : IBudgetGroupHeadCountCalculator
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;

		public BudgetGroupHeadCountSpecificationExtended(IScenarioRepository scenarioRepository, IBudgetDayRepository budgetDayRepository, IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister)
		{
			_scenarioRepository = scenarioRepository;
			_budgetDayRepository = budgetDayRepository;
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
		}

		public IValidatedRequest IsSatisfied(IAbsenceRequestAndSchedules absenceRequestSchedules)
		{
			var person = absenceRequestSchedules.AbsenceRequest.Person;
			var culture = person.PermissionInformation.Culture();
			var language = person.PermissionInformation.UICulture();
			var requestedPeriod = absenceRequestSchedules.AbsenceRequest.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			var personPeriod = person.Period(requestedPeriod.StartDate);

			if (personPeriod?.BudgetGroup == null)
			{
				return AbsenceRequestBudgetGroupValidationHelper.PersonPeriodOrBudgetGroupIsNull(culture, person.Id);
			}

			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			//
			var scheduleRange = absenceRequestSchedules.SchedulingResultStateHolder.Schedules[person];
			var periodToConsider = requestedPeriod;

			var scheduleDay = scheduleRange.ScheduledDay(periodToConsider.StartDate.AddDays(-1));
			var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
			if (visualLayerCollection.HasLayers)
			{
				var visualLayerCollectionPeriod = visualLayerCollection.Period();
				var absenceTimeWithinSchedule =
					absenceRequestSchedules.AbsenceRequest.Period.Intersection(visualLayerCollectionPeriod.Value);
				if (absenceTimeWithinSchedule.HasValue)
				{
					periodToConsider = new DateOnlyPeriod(periodToConsider.StartDate.AddDays(-1), periodToConsider.EndDate);
				}
			}

			var budgetDays = _budgetDayRepository.Find(defaultScenario, personPeriod.BudgetGroup, periodToConsider);
			if (budgetDays == null || budgetDays.Count == 0)
			{
				return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNull(language, culture, requestedPeriod);
			}

			//filtered budget days
			var budgetDaysToProcess = new List<IBudgetDay>();
			var periodHavingIntersectedShifts = new List<DateTimePeriod>();
			var daysNotHavingIntersectedShifts = new List<IBudgetDay>();
			
			foreach (var singleBudgetDay in budgetDays)
			{
				scheduleDay = scheduleRange.ScheduledDay(singleBudgetDay.Day);
				visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
				if (visualLayerCollection.HasLayers)
				{
					var visualLayerCollectionPeriod = visualLayerCollection.Period();
					var absenceTimeWithinSchedule = absenceRequestSchedules.AbsenceRequest.Period.Intersection(visualLayerCollectionPeriod.Value);
					if (absenceTimeWithinSchedule.HasValue)
					{
						periodHavingIntersectedShifts.Add(visualLayerCollectionPeriod.Value);
						budgetDaysToProcess.Add(singleBudgetDay);
					}
					else
					{
						daysNotHavingIntersectedShifts.Add(singleBudgetDay);
					}
				}
				else
				{
					daysNotHavingIntersectedShifts.Add(singleBudgetDay);
				}

			}

			foreach (var culpritDay in daysNotHavingIntersectedShifts)
			{
				var culpritDayPeriod = culpritDay.Day.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
				var periodFound = false;
				foreach (var coveredPeriod in periodHavingIntersectedShifts)
				{
					var intersectedPeriod = coveredPeriod.Intersection(culpritDayPeriod);
					if (intersectedPeriod.HasValue)
					{
						periodFound = true;
						break;
					}
				}

				if (!periodFound)
				{
					budgetDaysToProcess.Add(culpritDay);
				}

			}

			foreach (var day in periodToConsider.DayCollection())
			{
				scheduleDay = scheduleRange.ScheduledDay(day);
				visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
				if (visualLayerCollection.HasLayers)
				{
					var visualLayerCollectionPeriod = visualLayerCollection.Period();
					var absenceTimeWithinSchedule =
						absenceRequestSchedules.AbsenceRequest.Period.Intersection(visualLayerCollectionPeriod.Value);
					if (absenceTimeWithinSchedule.HasValue && !budgetDaysToProcess.Any(x => x.Day == day))
					{
						return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNotEqualToRequestedPeriodDays(language, culture,
							requestedPeriod);
					}
				}
			}

			var invalidDays = getInvalidDaysIfExist(budgetDaysToProcess, personPeriod.BudgetGroup, culture, absenceRequestSchedules.SchedulingResultStateHolder);
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