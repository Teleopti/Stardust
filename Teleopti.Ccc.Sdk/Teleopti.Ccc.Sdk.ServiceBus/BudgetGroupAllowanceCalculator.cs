using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class BudgetGroupAllowanceCalculator: IBudgetGroupAllowanceCalculator 
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BudgetGroupAllowanceCalculator));
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IBudgetDayRepository _budgetDayRepository;
        private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

        public BudgetGroupAllowanceCalculator(ISchedulingResultStateHolder schedulingResultStateHolder,
                                       IScenarioRepository scenarioRepository, IBudgetDayRepository budgetDayRepository,
                                       IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _scenarioRepository = scenarioRepository;
            _budgetDayRepository = budgetDayRepository;
            _scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public string CheckBudgetGroup(IAbsenceRequest absenceRequest)
        {
            var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
            var culture = absenceRequest.Person.PermissionInformation.Culture();
            var requestedPeriod = absenceRequest.Period.ToDateOnlyPeriod(timeZone);
            var personPeriod = absenceRequest.Person.PersonPeriods(requestedPeriod).FirstOrDefault();
            var invalidConfig = string.Empty;
            var invalidDays = string.Empty;
            var underStaffingValidationError = UserTexts.Resources.ResourceManager.GetString("InsufficientStaffingDays",culture);

            if (personPeriod == null || personPeriod.BudgetGroup == null)
            {
				Logger.DebugFormat("There is no budget group for you.");
				invalidConfig = UserTexts.Resources.ResourceManager.GetString("BudgetGroupMissing", culture);
                return invalidConfig;

            }

            var budgetGroup = personPeriod.BudgetGroup;
            var defaultScenario = _scenarioRepository.LoadDefaultScenario();
            var budgetDays = _budgetDayRepository.Find(defaultScenario, budgetGroup, requestedPeriod);
            
            if (budgetDays == null)
            {
                Logger.DebugFormat("There is no budget for this period {0}.", requestedPeriod);
                invalidConfig = string.Format(culture, UserTexts.Resources.NoBudgetForThisPeriod, requestedPeriod);
                return invalidConfig;
            }
            
            if (budgetDays.Count != requestedPeriod.DayCollection().Count)
            {
                Logger.DebugFormat("One or more days during this requested period {0} has no budget.", requestedPeriod);
                invalidConfig = string.Format(culture, UserTexts.Resources.NoBudgetDefineForSomeRequestedDays, requestedPeriod);
                return invalidConfig;
            }

            var scheduleRange = _schedulingResultStateHolder.Schedules[absenceRequest.Person];
            var count = 0;
            foreach (var budgetDay in budgetDays.OrderBy(x=>x.Day))
            {
                var currentDay = budgetDay.Day;
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
            }

            return invalidConfig;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public string CheckHeadCountInBudgetGroup(IAbsenceRequest absenceRequest)
        {
            var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
            var culture = absenceRequest.Person.PermissionInformation.Culture();
            var requestedPeriod = absenceRequest.Period.ToDateOnlyPeriod(timeZone);
            var personPeriod = absenceRequest.Person.PersonPeriods(requestedPeriod).FirstOrDefault();
            var invalidConfig = string.Empty;
            var invalidDays = string.Empty;
            string notEnoughAllowance = UserTexts.Resources.ResourceManager.GetString("NotEnoughAllowance", culture);

            if (personPeriod == null || personPeriod.BudgetGroup == null)
            {
                Logger.DebugFormat("There is no budget group for person: {0}.", absenceRequest.Person.Id);
                invalidConfig = string.Format(culture, UserTexts.Resources.NoBudgetGroupForPerson, absenceRequest.Person.Id);
                return invalidConfig;

            }

            var budgetGroup = personPeriod.BudgetGroup;
            var defaultScenario = _scenarioRepository.LoadDefaultScenario();
            var budgetDays = _budgetDayRepository.Find(defaultScenario, budgetGroup, requestedPeriod);

            if (budgetDays == null)
            {
                Logger.DebugFormat("There is no budget for this period {0}.", requestedPeriod);
                invalidConfig = string.Format(culture, UserTexts.Resources.NoBudgetForThisPeriod, requestedPeriod);
                return invalidConfig;
            }

            if (budgetDays.Count != requestedPeriod.DayCollection().Count)
            {
                Logger.DebugFormat("One or more days during this requested period {0} has no budget.", requestedPeriod);
                invalidConfig = string.Format(culture, UserTexts.Resources.NoBudgetDefineForSomeRequestedDays, requestedPeriod);
                return invalidConfig;
            }
            
            invalidDays = getInvalidDaysIfExist(budgetDays, budgetGroup, invalidDays, culture);

            if (!string.IsNullOrEmpty(invalidDays))
            {
                invalidDays = invalidDays.Substring(0, invalidDays.Length - 1);
                invalidConfig = notEnoughAllowance + invalidDays;
            }

            return invalidConfig;
        }

        private string getInvalidDaysIfExist(IEnumerable<IBudgetDay> budgetDays, IBudgetGroup budgetGroup, string invalidDays, CultureInfo culture)
        {
            var count = 0;
            foreach (var budgetDay in budgetDays.OrderBy(x => x.Day))
            {
                var currentDay = budgetDay.Day;
                var allowance = budgetDay.Allowance;
                var alreadyUsedAllowance =
                    _scheduleProjectionReadOnlyRepository.GetNumberOfAbsencesPerDayAndBudgetGroup(
                        budgetGroup.Id.GetValueOrDefault(), currentDay);

                if (Math.Floor(allowance) <= alreadyUsedAllowance)
                {
                    count++;
                    if (count > 5)
                        break;
                    Logger.DebugFormat("There is not enough allowance for day {0}.", budgetDay.Day);
                    invalidDays += budgetDay.Day.ToShortDateString(culture) + ",";
                }
            }
            return invalidDays;
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
                    requestedTime += absenceTimeWithinSchedule.Value.ElapsedTime();
            }
            else
            {
                var averageContractTimeSpan =
                    TimeSpan.FromMinutes(personPeriod.PersonContract.Contract.WorkTime.AvgWorkTimePerDay.TotalMinutes *
                                         personPeriod.PersonContract.PartTimePercentage.Percentage.Value);
                requestedTime += requestedPeriod.ElapsedTime() < averageContractTimeSpan
                                     ? requestedPeriod.ElapsedTime()
                                     : averageContractTimeSpan;
            }
            return requestedTime;
        }
    }
}
