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
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public class BudgetGroupAllowanceSpecification : PersonRequestSpecification<IAbsenceRequestAndSchedules>, IBudgetGroupAllowanceSpecification
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(BudgetGroupAllowanceSpecification));
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;

		public BudgetGroupAllowanceSpecification(ICurrentScenario scenarioRepository, IBudgetDayRepository budgetDayRepository,
			IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister)
		{
			_scenarioRepository = scenarioRepository;
			_budgetDayRepository = budgetDayRepository;
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
		}

		protected static bool IsSkillOpenForDateOnly(DateOnly date, IEnumerable<ISkill> skills)
		{
			return skills.Any(s => s.WorkloadCollection.Any(w => w.TemplateWeekCollection.Any(t => t.Key == (int)date.DayOfWeek && t.Value.OpenForWork.IsOpen)));
		}

		public override IValidatedRequest IsSatisfied(IAbsenceRequestAndSchedules absenceRequestAndSchedules)
		{
			var timeZone = absenceRequestAndSchedules.AbsenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var culture = absenceRequestAndSchedules.AbsenceRequest.Person.PermissionInformation.Culture();
			var language = absenceRequestAndSchedules.AbsenceRequest.Person.PermissionInformation.UICulture();

			var requestedPeriod = absenceRequestAndSchedules.AbsenceRequest.Period.ToDateOnlyPeriod(timeZone);
			var personPeriod = absenceRequestAndSchedules.AbsenceRequest.Person.PersonPeriods(requestedPeriod).FirstOrDefault();

			if (personPeriod?.BudgetGroup == null)
			{
				return AbsenceRequestBudgetGroupValidationHelper.PersonPeriodOrBudgetGroupIsNull(language, absenceRequestAndSchedules.AbsenceRequest.Person.Id);
			}

			var defaultScenario = _scenarioRepository.Current();
			var budgetDays = _budgetDayRepository.Find(defaultScenario, personPeriod.BudgetGroup, requestedPeriod,true);
			
			if (budgetDays == null)
			{
				return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNull(language, culture, requestedPeriod);
			}

			var filteredBudgetDays = budgetDays.GroupBy(a => a.Day)
				.Select(b => b.OrderByDescending(c => ((BudgetDay)c).UpdatedOn).First())
				.ToList();

			if (filteredBudgetDays.Count != requestedPeriod.DayCount())
			{
				return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNotEqualToRequestedPeriodDays(language, culture, requestedPeriod);
			}

			var invalidDays = getInvalidDays(absenceRequestAndSchedules.AbsenceRequest, filteredBudgetDays, personPeriod, defaultScenario, culture, absenceRequestAndSchedules.SchedulingResultStateHolder);
			if (!string.IsNullOrEmpty(invalidDays))
			{
				var underStaffingValidationError = UserTexts.Resources.ResourceManager.GetString("InsufficientStaffingDays", language);
				return AbsenceRequestBudgetGroupValidationHelper.InvalidDaysInBudgetDays(invalidDays, underStaffingValidationError);
			}

			return new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty };
		}

		private string getInvalidDays(IAbsenceRequest absenceRequest, IList<IBudgetDay> budgetDays, IPersonPeriod personPeriod,
			IScenario defaultScenario, CultureInfo culture, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var scheduleRange = schedulingResultStateHolder.Schedules[absenceRequest.Person];
			var count = 0;
			var invalidDays = string.Empty;
			var alreayAddedMinuteDictionary = new Dictionary<IBudgetDay, double>();

			foreach (var budgetDay in budgetDays.OrderBy(x => x.Day))
			{
				var currentDay = budgetDay.Day;
				var remainingAllowanceMinutes = getRemainingAllowanceMinutes(personPeriod, defaultScenario, budgetDay, currentDay);
				var requestedAbsenceMinutes = calculateRequestedMinutes(currentDay, absenceRequest.Period, scheduleRange, personPeriod).TotalMinutes;
				var addedBefore = schedulingResultStateHolder.AddedAbsenceMinutesDuringCurrentRequestHandlingCycle(budgetDay);
				remainingAllowanceMinutes -= addedBefore;

				if (remainingAllowanceMinutes < requestedAbsenceMinutes)
				{
					if (alreayAddedMinuteDictionary.Any())
					{
						alreayAddedMinuteDictionary.ForEach(item =>
						{
							schedulingResultStateHolder
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
					schedulingResultStateHolder.AddAbsenceMinutesDuringCurrentRequestHandlingCycle(budgetDay, requestedAbsenceMinutes);
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

		private static TimeSpan calculateRequestedMinutes(DateOnly currentDay, DateTimePeriod requestedPeriod,
			IScheduleRange scheduleRange, IPersonPeriod personPeriod)
		{
			var requestedTime = TimeSpan.Zero;
			var scheduleDay = scheduleRange.ScheduledDay(currentDay);
			var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
			var visualLayerCollectionPeriod = visualLayerCollection.Period();

			if (scheduleDay.IsScheduled() && visualLayerCollectionPeriod.HasValue)
			{
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