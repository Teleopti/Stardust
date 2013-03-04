using System;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
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

        public string CheckBudgetGroup(IAbsenceRequest obj)
        {
            var timeZone = obj.Person.PermissionInformation.DefaultTimeZone();
            var culture = obj.Person.PermissionInformation.Culture();
            var requestedPeriod = obj.Period.ToDateOnlyPeriod(timeZone);
            var personPeriod = obj.Person.PersonPeriods(requestedPeriod).FirstOrDefault();
            var invalidConfig = "";
            var invalidDays = "";

            if (personPeriod == null || personPeriod.BudgetGroup == null)
            {
                Logger.DebugFormat("There is no budget group for person: {0}.", obj.Person.Id);
                invalidConfig = string.Format(obj.Person.PermissionInformation.Culture(), UserTexts.Resources.NoBudgetGroupForPerson, obj.Person.Id);
                return invalidConfig;

            }

            var budgetGroup = personPeriod.BudgetGroup;
            var defaultScenario = _scenarioRepository.LoadDefaultScenario();
            var budgetDays = _budgetDayRepository.Find(defaultScenario, budgetGroup, requestedPeriod);
            
            if (budgetDays == null)
            {
                Logger.DebugFormat("There is no budget for this period {0}.", requestedPeriod);
                invalidConfig = string.Format(obj.Person.PermissionInformation.Culture(), UserTexts.Resources.NoBudgetForThisPeriod, requestedPeriod);
                return invalidConfig;
            }
            
            if (budgetDays.Count != requestedPeriod.DayCollection().Count)
            {
                Logger.DebugFormat("One or more days during this requested period {0} has no budget.", requestedPeriod);
                invalidConfig = string.Format(obj.Person.PermissionInformation.Culture(), UserTexts.Resources.NoBudgetDefineForSomeRequestedDays, requestedPeriod);
                return invalidConfig;
            }

            var scheduleRange = _schedulingResultStateHolder.Schedules[obj.Person];
            foreach (var budgetDay in budgetDays)
            {
                var currentDay = budgetDay.Day;
                var allowanceMinutes = budgetDay.Allowance * budgetDay.FulltimeEquivalentHours * TimeDefinition.MinutesPerHour;
                var usedAbsenceMinutes = TimeSpan.FromTicks(
                    _scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(currentDay, currentDay),
                        budgetGroup, defaultScenario).Sum(p => p.TotalContractTime)).TotalMinutes;
                var remainingAllowanceMinutes = allowanceMinutes - usedAbsenceMinutes;
                var requestedAbsenceMinutes = calculateRequestedMinutes(currentDay, obj.Period, scheduleRange, personPeriod).TotalMinutes;
                if (remainingAllowanceMinutes < requestedAbsenceMinutes)
                {
                    Logger.DebugFormat(
                        "There is not enough allowance for day {0}. The remaining allowance is {1} hours, but you request for {2} hours",
                        budgetDay.Day, remainingAllowanceMinutes / TimeDefinition.MinutesPerHour,
                        requestedAbsenceMinutes / TimeDefinition.MinutesPerHour);
                    invalidDays += currentDay.ToShortDateString(culture) + ",";
                }
            }

            if (invalidDays != "")
            {
                invalidDays = invalidDays.Substring(0, invalidDays.Length - 1);
                invalidConfig = string.Format(culture,
                                              UserTexts.Resources.InsufficientStaffingDays, invalidDays);
            }

            return invalidConfig;
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
