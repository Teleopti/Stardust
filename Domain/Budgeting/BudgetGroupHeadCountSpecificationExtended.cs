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
			var timezone = person.PermissionInformation.DefaultTimeZone();
			var requestedPeriod = absenceRequestSchedules.AbsenceRequest.Period.ToDateOnlyPeriod(timezone);
			var personPeriod = person.Period(requestedPeriod.StartDate);
			var absenceReqPeriodInUserTimezone = convertToDateTimePeriod(absenceRequestSchedules.AbsenceRequest.Period, timezone);
			if (personPeriod?.BudgetGroup == null)
			{
				return AbsenceRequestBudgetGroupValidationHelper.PersonPeriodOrBudgetGroupIsNull(culture, person.Id);
			}

			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			//
			var scheduleRange = absenceRequestSchedules.SchedulingResultStateHolder.Schedules[person];
			var periodToConsider = requestedPeriod;

			if (!requestIsFullDayOrMultiDay(absenceRequestSchedules.AbsenceRequest.Period))
			{
				var scheduleDay = scheduleRange.ScheduledDay(periodToConsider.StartDate.AddDays(-1));
				var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
				if (visualLayerCollection.HasLayers)
				{
					var visualLayerCollectionPeriodInUserTimeZone = convertToDateTimePeriod(visualLayerCollection.Period().Value, timezone);
					var absenceTimeWithinSchedule = absenceReqPeriodInUserTimezone.Intersection(visualLayerCollectionPeriodInUserTimeZone);
					if (absenceTimeWithinSchedule.HasValue)
					{
						periodToConsider = new DateOnlyPeriod(periodToConsider.StartDate.AddDays(-1), periodToConsider.EndDate);
					}
				}
			}
			

			var budgetDays = _budgetDayRepository.Find(defaultScenario, personPeriod.BudgetGroup, periodToConsider);
			if (budgetDays == null || budgetDays.Count == 0)
			{
				return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNull(language, culture, requestedPeriod);
			}

			var budgetDaysToProcess = new List<IBudgetDay>();
			var dayToProcess = new List<DateOnly>();
			var stayAwayFromTheseDays = new List<DateOnly>();
			foreach (var day in periodToConsider.DayCollection())
			{
				var scheduleDay = scheduleRange.ScheduledDay(day);
				var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
				if (visualLayerCollection.HasLayers)
				{
					var visualLayerCollectionPeriodInUserTimeZone = convertToDateTimePeriod(visualLayerCollection.Period().Value ,timezone);
					var absenceTimeWithinSchedule = absenceReqPeriodInUserTimezone.Intersection(visualLayerCollectionPeriodInUserTimeZone);
					if (absenceTimeWithinSchedule.HasValue)
					{
						if (!budgetDays.Any(x => x.Day == day))
						{
							return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNotEqualToRequestedPeriodDays(language, culture,
								requestedPeriod);
						}
						dayToProcess.Add(visualLayerCollectionPeriodInUserTimeZone.StartDateTime.ToDateOnly());
						foreach (var stayAwayFromThisDay in visualLayerCollectionPeriodInUserTimeZone.ToDateOnlyPeriod(timezone).DayCollection())
						{
							stayAwayFromTheseDays.Add(stayAwayFromThisDay);
						}
						
					}
					else if(requestedPeriod.DayCollection().Any(x => x == day) && !stayAwayFromTheseDays.Contains(day))
					{
						dayToProcess.Add(day);
					}
					
				}
				else
				{
					if (requestedPeriod.DayCollection().Any(x => x == day) && !stayAwayFromTheseDays.Contains(day))
					{
						dayToProcess.Add(day);
					}
				}
			}

			foreach (var dateOnly in dayToProcess)
			{
				var thatDay = budgetDays.FirstOrDefault(x => x.Day == dateOnly);
				if(thatDay!=null)
					budgetDaysToProcess.Add(thatDay);
			}
			
			var invalidDays = getInvalidDaysIfExist(budgetDaysToProcess, personPeriod.BudgetGroup, culture, absenceRequestSchedules.BudgetGroupState);
			if (!string.IsNullOrEmpty(invalidDays))
			{
				var notEnoughAllowance = Resources.ResourceManager.GetString(nameof(Resources.NotEnoughBudgetAllowanceForTheDay), language);
				return AbsenceRequestBudgetGroupValidationHelper.InvalidDaysInBudgetDays(invalidDays, notEnoughAllowance);
			}

			return new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty };
		}

		private bool requestIsFullDayOrMultiDay(DateTimePeriod absenceRequestPeriod)
		{
			return absenceRequestPeriod.ElapsedTime().Ticks >= (new TimeSpan(0, 23, 59, 0)).Ticks;
		}


		private DateTimePeriod convertToDateTimePeriod(DateTimePeriod dateTimePeriod, TimeZoneInfo timezone)
		{
			var startDateTime = TimeZoneHelper.ConvertFromUtc(dateTimePeriod.StartDateTime, timezone);
			var endDateTime = TimeZoneHelper.ConvertFromUtc(dateTimePeriod.EndDateTime, timezone);
			return new DateTimePeriod(new DateTime(startDateTime.Ticks, DateTimeKind.Utc), new DateTime(endDateTime.Ticks, DateTimeKind.Utc));
		}

		private string getInvalidDaysIfExist(IEnumerable<IBudgetDay> budgetDays, IBudgetGroup budgetGroup, CultureInfo culture, BudgetGroupState budgetGroupState)
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
				var addedBefore = budgetGroupState.AddedAbsenceHeadCountDuringCurrentRequestHandlingCycle(budgetDay);
				alreadyUsedAllowance += addedBefore;
				if (Math.Floor(allowance) <= alreadyUsedAllowance)
				{
					if (alreayAddedHeadCountDictionary.Any())
					{
						alreayAddedHeadCountDictionary.ForEach(item =>
						{
							budgetGroupState
								.SubtractAbsenceHeadCountDuringCurrentRequestHandlingCycle(item.Key);
						});
					}

					count++;
					if (count > 5) break;
					invalidDays += currentDay.ToShortDateString(culture) + ",";
				}
				else
				{
					budgetGroupState.AddAbsenceHeadCountDuringCurrentRequestHandlingCycle(budgetDay);
					alreayAddedHeadCountDictionary.Add(budgetDay, 1);
				}
			}
			return invalidDays.IsEmpty() ? invalidDays : invalidDays.Substring(0, invalidDays.Length - 1);
		}
	}
}