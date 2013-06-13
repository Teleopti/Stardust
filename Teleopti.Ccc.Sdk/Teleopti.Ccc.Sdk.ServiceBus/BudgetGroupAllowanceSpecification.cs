using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override bool IsSatisfiedBy(IAbsenceRequest obj)
        {
            var timeZone = obj.Person.PermissionInformation.DefaultTimeZone();
            var requestedPeriod = obj.Period.ToDateOnlyPeriod(timeZone);
            var personPeriod = obj.Person.PersonPeriods(requestedPeriod).FirstOrDefault();

            if (personPeriod == null || personPeriod.BudgetGroup == null)
			{
                Logger.DebugFormat("There is no budget group for person: {0}.", obj.Person.Id);
                return false;
            }

			var budgetGroup = personPeriod.BudgetGroup;
            var defaultScenario = _scenarioRepository.Current();
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

        	var scheduleRange = _schedulingResultStateHolder.Schedules[obj.Person];
            foreach (var budgetDay in budgetDays)
            {
                var currentDay = budgetDay.Day;

                if(budgetDay.IsClosed)
                    continue;

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
                    return false;
                }
            }
            return true;
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


    }
}
