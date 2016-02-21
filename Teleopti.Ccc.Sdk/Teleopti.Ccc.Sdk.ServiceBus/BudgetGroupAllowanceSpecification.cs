using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Sdk.ServiceBus.AbsenceRequest;
using Teleopti.Interfaces.Domain;
using log4net;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class BudgetGroupAllowanceSpecification : PersonRequestSpecification<IAbsenceRequest>, IBudgetGroupAllowanceSpecification
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(BudgetGroupAllowanceSpecification));

		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

		public BudgetGroupAllowanceSpecification(ISchedulingResultStateHolder schedulingResultStateHolder, ICurrentScenario scenarioRepository, IBudgetDayRepository budgetDayRepository, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_scenarioRepository = scenarioRepository;
			_budgetDayRepository = budgetDayRepository;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		}

		protected static bool IsSkillOpenForDateOnly(DateOnly date, IEnumerable<ISkill> skills)
		{
			return skills.Any(s => s.WorkloadCollection.Any(w => w.TemplateWeekCollection.Any(t => t.Key == (int)date.DayOfWeek && t.Value.OpenForWork.IsOpen)));
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

			var defaultScenario = _scenarioRepository.Current();
			var budgetDays = _budgetDayRepository.Find(defaultScenario, personPeriod.BudgetGroup, requestedPeriod);

			if (budgetDays == null)
			{
				return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNull(culture, requestedPeriod);
			}

			if (budgetDays.Count != requestedPeriod.DayCollection().Count)
			{
				return AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNotEqualToRequestedPeriodDays(culture, requestedPeriod);
			}

			var invalidDays = getInvalidDays(absenceRequest, budgetDays, personPeriod, defaultScenario, culture);
			if (!string.IsNullOrEmpty(invalidDays))
			{
				var underStaffingValidationError = UserTexts.Resources.ResourceManager.GetString("InsufficientStaffingDays", culture);
				return AbsenceRequestBudgetGroupValidationHelper.InvalidDaysInBudgetDays(invalidDays, underStaffingValidationError);
			}

			return new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty };
		}



		private string getInvalidDays(IAbsenceRequest absenceRequest, IList<IBudgetDay> budgetDays, IPersonPeriod personPeriod, IScenario defaultScenario, CultureInfo culture)
		{
			var scheduleRange = _schedulingResultStateHolder.Schedules[absenceRequest.Person];
			var count = 0;
			var invalidDays = string.Empty;

			foreach (var budgetDay in budgetDays.OrderBy(x => x.Day))
			{
				if (budgetDay.IsClosed) continue;

				var currentDay = budgetDay.Day;
				var remainingAllowanceMinutes = getRemainingAllowanceMinutes(personPeriod, defaultScenario, budgetDay, currentDay);
				var requestedAbsenceMinutes = calculateRequestedMinutes(currentDay, absenceRequest.Period, scheduleRange, personPeriod).TotalMinutes;

				if (remainingAllowanceMinutes < requestedAbsenceMinutes)
				{
					// only showing first 5 days if a person request conatins more than 5 days.
					count++;
					if (count > 5) break;

					Logger.DebugFormat(	"There is not enough allowance for day {0}. The remaining allowance is {1} hours, but you request for {2} hours",
										currentDay, 
										remainingAllowanceMinutes / TimeDefinition.MinutesPerHour,
										requestedAbsenceMinutes / TimeDefinition.MinutesPerHour);
					invalidDays += currentDay.ToShortDateString(culture) + ",";
				}
			}
			return invalidDays.IsEmpty() ? invalidDays : invalidDays.Substring(0, invalidDays.Length - 1);
		}

		private double getRemainingAllowanceMinutes(IPersonPeriod personPeriod, IScenario defaultScenario, IBudgetDay budgetDay, DateOnly currentDay)
		{
			var allowanceMinutes = budgetDay.Allowance * budgetDay.FulltimeEquivalentHours * TimeDefinition.MinutesPerHour;
			var absenceTimeListForAllBudgetGroup = _scheduleProjectionReadOnlyRepository
													.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(currentDay, currentDay), personPeriod.BudgetGroup, defaultScenario);

			var usedAbsenceMinutes = TimeSpan.FromTicks(absenceTimeListForAllBudgetGroup.Sum(p => p.TotalContractTime)).TotalMinutes;
			var remainingAllowanceMinutes = allowanceMinutes - usedAbsenceMinutes;
			return remainingAllowanceMinutes;
		}

		private static TimeSpan calculateRequestedMinutes(DateOnly currentDay, DateTimePeriod requestedPeriod, IScheduleRange scheduleRange, IPersonPeriod personPeriod)
		{
			var requestedTime = TimeSpan.Zero;
			var scheduleDay = scheduleRange.ScheduledDay(currentDay);
			var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
			var visualLayerCollectionPeriod = visualLayerCollection.Period();

			if (scheduleDay.IsScheduled() && visualLayerCollectionPeriod.HasValue)
			{
				var absenceTimeWithinSchedule = requestedPeriod.Intersection(visualLayerCollectionPeriod.Value);
				if (absenceTimeWithinSchedule.HasValue)
					requestedTime += visualLayerCollection.ContractTime(visualLayerCollectionPeriod.GetValueOrDefault());
			}
			else
			{
				var averageContractTimeSpan = TimeSpan.FromMinutes( personPeriod.PersonContract.Contract.WorkTime.AvgWorkTimePerDay.TotalMinutes 
																  * personPeriod.PersonContract.PartTimePercentage.Percentage.Value);
				requestedTime += requestedPeriod.ElapsedTime() < averageContractTimeSpan
									 ? requestedPeriod.ElapsedTime()
									 : averageContractTimeSpan;
			}
			return requestedTime;
		}
	}
}
