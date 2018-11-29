using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public class BudgetGroupAllowanceCalculatorExtended : IBudgetGroupAllowanceCalculator
	{
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(BudgetGroupAllowanceCalculatorExtended));

		public BudgetGroupAllowanceCalculatorExtended(ICurrentScenario scenarioRepository, IBudgetDayRepository budgetDayRepository, IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister)
		{
			_scenarioRepository = scenarioRepository;
			_budgetDayRepository = budgetDayRepository;
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
		}

		public  IValidatedRequest IsSatisfied(IAbsenceRequestAndSchedules absenceRequestAndSchedules)
		{
			var person = absenceRequestAndSchedules.AbsenceRequest.Person;
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var culture = person.PermissionInformation.Culture();
			var language = person.PermissionInformation.UICulture();
			var requestedPeriod = absenceRequestAndSchedules.AbsenceRequest.Period.ToDateOnlyPeriod(timeZone);
			var personPeriod = person.PersonPeriods(requestedPeriod).FirstOrDefault();

			if (personPeriod?.BudgetGroup == null)
			{
				return AbsenceRequestBudgetGroupValidationHelper.PersonPeriodOrBudgetGroupIsNull(language, person.Id);
			}

			var defaultScenario = _scenarioRepository.Current();
			var scheduleRange = absenceRequestAndSchedules.SchedulingResultStateHolder.Schedules[person];
			var periodToConsider = requestedPeriod;
			//special case
			if (absenceRequestAndSchedules.AbsenceRequest.Period.ElapsedTime() < (new TimeSpan(0, 23, 59, 0)))
			{
				var scheduleDay = scheduleRange.ScheduledDay(periodToConsider.StartDate.AddDays(-1));
				var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
				if (visualLayerCollection.HasLayers)
				{
					var visualLayerCollectionPeriod = visualLayerCollection.Period();
					var absenceTimeWithinSchedule = absenceRequestAndSchedules.AbsenceRequest.Period.Intersection(visualLayerCollectionPeriod.Value);
					if (absenceTimeWithinSchedule.HasValue)
					{
						periodToConsider = new DateOnlyPeriod(periodToConsider.StartDate.AddDays(-1), periodToConsider.EndDate);
					}
				}
				
			}
			
			var budgetDays = _budgetDayRepository.Find(defaultScenario, personPeriod.BudgetGroup, periodToConsider, true);

			if (budgetDays == null)
			{
				return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNull(language, culture, periodToConsider);
			}

			var filteredBudgetDays = budgetDays.GroupBy(a => a.Day)
				.Select(b => b.OrderByDescending(c => ((BudgetDay)c).UpdatedOn).First())
				.ToList();

			if (filteredBudgetDays.Count != periodToConsider.DayCount())
			{
				return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNotEqualToRequestedPeriodDays(language, culture, periodToConsider);
			}

			var invalidDays = getInvalidDays(absenceRequestAndSchedules.AbsenceRequest, filteredBudgetDays, personPeriod, defaultScenario, culture, absenceRequestAndSchedules.SchedulingResultStateHolder, absenceRequestAndSchedules.BudgetGroupState);
			if (!string.IsNullOrEmpty(invalidDays))
			{
				var notEnoughAllowance = Resources.ResourceManager.GetString(nameof(Resources.NotEnoughBudgetAllowanceForTheDay), language);
				return AbsenceRequestBudgetGroupValidationHelper.InvalidDaysInBudgetDays(invalidDays, notEnoughAllowance);
			}

			return new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty };
		}

		private string getInvalidDays(IAbsenceRequest absenceRequest, IList<IBudgetDay> budgetDays, IPersonPeriod personPeriod,
			IScenario defaultScenario, CultureInfo culture, ISchedulingResultStateHolder schedulingResultStateHolder, BudgetGroupState budgetGroupState)
		{
			var scheduleRange = schedulingResultStateHolder.Schedules[absenceRequest.Person];
			var count = 0;
			var invalidDays = string.Empty;
			var alreayAddedMinuteDictionary = new Dictionary<IBudgetDay, double>();

			foreach (var budgetDay in budgetDays.OrderBy(x => x.Day))
			{
				var currentDay = budgetDay.Day;
				
				//special case
				var scheduleDay = scheduleRange.ScheduledDay(currentDay);
				var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
				if (!visualLayerCollection.HasLayers)
					continue;
				if (visualLayerCollection.HasLayers)
				{
					var visualLayerCollectionPeriod = visualLayerCollection.Period();
					var absenceTimeWithinSchedule = absenceRequest.Period.Intersection(visualLayerCollectionPeriod.Value);
					if (!absenceTimeWithinSchedule.HasValue)
					{
						continue;
					}
				}

				var remainingAllowanceMinutes = getRemainingAllowanceMinutes(personPeriod, defaultScenario, budgetDay, currentDay);
				var requestedAbsenceMinutes = calculateRequestedMinutes(currentDay, absenceRequest.Period, scheduleRange, personPeriod, absenceRequest.Person).TotalMinutes;
				var addedBefore = budgetGroupState.AddedAbsenceMinutesDuringCurrentRequestHandlingCycle(budgetDay);
				remainingAllowanceMinutes -= addedBefore;

				if (remainingAllowanceMinutes < requestedAbsenceMinutes)
				{
					if (alreayAddedMinuteDictionary.Any())
					{
						alreayAddedMinuteDictionary.ForEach(item =>
						{
							budgetGroupState
								.SubtractAbsenceMinutesDuringCurrentRequestHandlingCycle(item.Key, item.Value);
						});
					}

					// only showing first 5 days if a person request conatins more than 5 days.
					count++;
					if (count > 5) break;

					Logger.DebugFormat(
						"There is not enough allowance for day {0}. The remaining allowance is {1} hours, but you request for {2} hours",
						currentDay,
						remainingAllowanceMinutes / TimeDefinition.MinutesPerHour,
						requestedAbsenceMinutes / TimeDefinition.MinutesPerHour);
					invalidDays += currentDay.ToShortDateString(culture) + ",";
				}
				else
				{
					budgetGroupState.AddAbsenceMinutesDuringCurrentRequestHandlingCycle(budgetDay, requestedAbsenceMinutes);
					alreayAddedMinuteDictionary.Add(budgetDay, requestedAbsenceMinutes);
				}
			}
			return invalidDays.IsEmpty() ? invalidDays : invalidDays.Substring(0, invalidDays.Length - 1);
		}

		private double getRemainingAllowanceMinutes(IPersonPeriod personPeriod, IScenario defaultScenario, IBudgetDay budgetDay, DateOnly currentDay)
		{
			var allowanceMinutes = budgetDay.ShrinkedAllowance * budgetDay.FulltimeEquivalentHours * TimeDefinition.MinutesPerHour;
			var absenceTimeListForAllBudgetGroup = _scheduleProjectionReadOnlyPersister
				.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(currentDay, currentDay), personPeriod.BudgetGroup, defaultScenario);

			var usedAbsenceMinutes = TimeSpan.FromTicks(absenceTimeListForAllBudgetGroup.Sum(p => p.TotalContractTime)).TotalMinutes;
			var remainingAllowanceMinutes = allowanceMinutes - usedAbsenceMinutes;
			return remainingAllowanceMinutes;
		}

		private TimeSpan calculateRequestedMinutes(DateOnly currentDay, DateTimePeriod requestedPeriod,
			IScheduleRange scheduleRange, IPersonPeriod personPeriod, IPerson person)
		{
			var requestedTime = TimeSpan.Zero;
			var scheduleDay = scheduleRange.ScheduledDay(currentDay);
			var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
			var visualLayerCollectionPeriod = visualLayerCollection.Period();

			if (scheduleDay.IsScheduled() && visualLayerCollectionPeriod.HasValue)
			{
				//special case
				var intersectedPeriod = requestedPeriod.Intersection(currentDay.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()));
				if (intersectedPeriod.HasValue && intersectedPeriod.Value.ElapsedTime().Equals(new TimeSpan(0, 23, 59, 0)))
				{
					requestedPeriod = new DateTimePeriod(requestedPeriod.StartDateTime, visualLayerCollectionPeriod.Value.EndDateTime);
				}
				var absenceTimeWithinSchedule = requestedPeriod.Intersection(visualLayerCollectionPeriod.Value);
				if (absenceTimeWithinSchedule.HasValue)
				{
					requestedTime += visualLayerCollection.ContractTime(absenceTimeWithinSchedule.Value);
				}
			}
			else
			{
				var personContract = personPeriod.PersonContract;
				var averageWorktimePerDayInMinutes = personContract.Contract.WorkTime.AvgWorkTimePerDay.TotalMinutes;
				var partTimePercentage = personContract.PartTimePercentage.Percentage.Value;
				var averageContractTimeSpan = TimeSpan.FromMinutes(averageWorktimePerDayInMinutes * partTimePercentage);

				requestedTime += requestedPeriod.ElapsedTime() < averageContractTimeSpan
					? requestedPeriod.ElapsedTime()
					: averageContractTimeSpan;
			}
			return requestedTime;
		}
	}
}