using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class BudgetGroupAllowanceSpecification : Specification<IAbsenceRequest>, IBudgetGroupAllowanceSpecification
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BudgetGroupAllowanceSpecification));

        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IScenarioProvider _scenarioProvider;
        private readonly IBudgetDayRepository _budgetDayRepository;
        private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

        public BudgetGroupAllowanceSpecification(ISchedulingResultStateHolder schedulingResultStateHolder, IScenarioProvider scenarioProvider, IBudgetDayRepository budgetDayRepository, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _scenarioProvider = scenarioProvider;
            _budgetDayRepository = budgetDayRepository;
            _scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override bool IsSatisfiedBy(IAbsenceRequest obj)
        {
            var timeZone = obj.Person.PermissionInformation.DefaultTimeZone();
            var requestedPeriod = obj.Period.ToDateOnlyPeriod(timeZone);
            var personPeriod = obj.Person.PersonPeriodCollection.Where(
                p => p.Period.Contains(requestedPeriod)).FirstOrDefault();
            IBudgetGroup budgetGroup = null;
            if (personPeriod != null && personPeriod.BudgetGroup != null)
                budgetGroup = personPeriod.BudgetGroup;
            if (budgetGroup == null)
            {
                Logger.DebugFormat("There is no budget group for person: {0}.", obj.Person.Id);
                return false;
            }

            var defaultScenario = _scenarioProvider.DefaultScenario();
            var absencesInBudgetGroup = new HashSet<IAbsence>();
            foreach (var budgetAbsence in budgetGroup.CustomShrinkages.SelectMany(customShrinkage => customShrinkage.BudgetAbsenceCollection))
                absencesInBudgetGroup.Add(budgetAbsence);
            var budgetDays = _budgetDayRepository.Find(defaultScenario, budgetGroup, requestedPeriod);
            if (budgetDays == null)
            {
                Logger.DebugFormat("There is no budget for this period {0}.", requestedPeriod);
                return false;
            }
            if (budgetDays.Count != requestedPeriod.DayCollection().Count)
            {
                Logger.DebugFormat("One or more days during this requested period {0} has no budget.", requestedPeriod);
                return false;
            }
            foreach (var budgetDay in budgetDays)
            {
                var currentDay = budgetDay.Day;
                var allowanceMinutes = budgetDay.Allowance * budgetDay.FulltimeEquivalentHours * TimeDefinition.MinutesPerHour;
                var usedAbsenceMinutes = TimeSpan.FromTicks(
                    _scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(currentDay, currentDay),
                        budgetGroup, defaultScenario).Sum(p => p.TotalContractTime)).TotalMinutes;
                var remainingAllowanceMinutes = allowanceMinutes - usedAbsenceMinutes;
                var requestedAbsenceMiniutes = calculateRequestedMinutes(currentDay, obj.Period, obj.Person, personPeriod).TotalMinutes;
                if (remainingAllowanceMinutes < requestedAbsenceMiniutes)
                {
                    Logger.DebugFormat(
                        "There is not enough allowance for day {0}. The remaining allowance is {1} hours, but you request for {2} hours",
                        budgetDay.Day, remainingAllowanceMinutes / TimeDefinition.MinutesPerHour,
                        requestedAbsenceMiniutes / TimeDefinition.MinutesPerHour);
                    return false;
                }
            }
            return true;
        }

        private TimeSpan calculateRequestedMinutes(DateOnly currentDay, DateTimePeriod requestedPeriod, IPerson person, IPersonPeriod personPeriod)
        {
            var requestedTime = TimeSpan.Zero;
            var scheduleDay = _schedulingResultStateHolder.Schedules[person].ScheduledDay(currentDay);
            var res = scheduleDay.ProjectionService().CreateProjection();

            if (scheduleDay.IsScheduled() && res.Period().HasValue)
            {
                var absenceTimeWithinSchedule = requestedPeriod.Intersection(res.Period().GetValueOrDefault());
                if (absenceTimeWithinSchedule.HasValue)
                    requestedTime += absenceTimeWithinSchedule.GetValueOrDefault().ElapsedTime();
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
    }
}
