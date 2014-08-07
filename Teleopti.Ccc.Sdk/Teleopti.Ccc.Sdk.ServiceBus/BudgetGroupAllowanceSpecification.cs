using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;
using log4net;

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
                var averageContractTimeSpan =
                    TimeSpan.FromMinutes(personPeriod.PersonContract.Contract.WorkTime.AvgWorkTimePerDay.TotalMinutes*
                                         personPeriod.PersonContract.PartTimePercentage.Percentage.Value);
                requestedTime += requestedPeriod.ElapsedTime() < averageContractTimeSpan
                                     ? requestedPeriod.ElapsedTime()
                                     : averageContractTimeSpan;
            }
            return requestedTime;
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
            string invalidConfig;
            var invalidDays = string.Empty;
            var underStaffingValidationError = UserTexts.Resources.ResourceManager.GetString("InsufficientStaffingDays", culture);
            var validatedRequest = new ValidatedRequest{IsValid = true, ValidationErrors = string.Empty};
            
            if (personPeriod == null || personPeriod.BudgetGroup == null)
            {
								Logger.DebugFormat("There is no budget group for you: {0}", absenceRequest.Person.Id);
								invalidConfig = UserTexts.Resources.ResourceManager.GetString("BudgetGroupMissing", culture); 
		            validatedRequest.IsValid = false;
                validatedRequest.ValidationErrors = invalidConfig;
                return validatedRequest;

            }

            var budgetGroup = personPeriod.BudgetGroup;
            var defaultScenario = _scenarioRepository.Current();
            var budgetDays = _budgetDayRepository.Find(defaultScenario, budgetGroup, requestedPeriod);

            if (budgetDays == null)
            {
                Logger.DebugFormat("There is no budget for this period {0}.", requestedPeriod);
                invalidConfig = string.Format(culture, UserTexts.Resources.NoBudgetForThisPeriod, requestedPeriod);
                validatedRequest.IsValid = false;
                validatedRequest.ValidationErrors = invalidConfig;
                return validatedRequest;
            }

            if (budgetDays.Count != requestedPeriod.DayCollection().Count)
            {
                Logger.DebugFormat("One or more days during this requested period {0} has no budget.", requestedPeriod);
                invalidConfig = string.Format(culture, UserTexts.Resources.NoBudgetDefineForSomeRequestedDays, requestedPeriod);
                validatedRequest.IsValid = false;
                validatedRequest.ValidationErrors = invalidConfig;
                return validatedRequest;
            }

            var scheduleRange = _schedulingResultStateHolder.Schedules[absenceRequest.Person];
            var count = 0;
            foreach (var budgetDay in budgetDays.OrderBy(x => x.Day))
            {
                var currentDay = budgetDay.Day;

                if (budgetDay.IsClosed)
                    continue;

                var allowanceMinutes = budgetDay.Allowance * budgetDay.FulltimeEquivalentHours * TimeDefinition.MinutesPerHour;
                var usedAbsenceMinutes = TimeSpan.FromTicks(
                    _scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(currentDay, currentDay),
                        budgetGroup, defaultScenario).Sum(p => p.TotalContractTime)).TotalMinutes;
                var remainingAllowanceMinutes = allowanceMinutes - usedAbsenceMinutes;
                var requestedAbsenceMinutes = calculateRequestedMinutes(currentDay, absenceRequest.Period, scheduleRange, personPeriod).TotalMinutes;
                if (remainingAllowanceMinutes < requestedAbsenceMinutes)
                {
                    // only showing first 5 days if a person request conatins more than 5 days.
                    count++;
                    if (count > 5)
                        break;

                    Logger.DebugFormat(
                        "There is not enough allowance for day {0}. The remaining allowance is {1} hours, but you request for {2} hours",
                        budgetDay.Day, remainingAllowanceMinutes / TimeDefinition.MinutesPerHour,
                        requestedAbsenceMinutes / TimeDefinition.MinutesPerHour);
                    invalidDays += budgetDay.Day.ToShortDateString(culture) + ",";
                }
            }

            if (!string.IsNullOrEmpty(invalidDays))
            {
                invalidDays = invalidDays.Substring(0, invalidDays.Length - 1);
                invalidConfig = underStaffingValidationError + invalidDays;

                if(!String.IsNullOrEmpty(invalidConfig))
                    return new ValidatedRequest { IsValid = false, ValidationErrors = invalidConfig };
            }

            return validatedRequest;
        }
    }
}
