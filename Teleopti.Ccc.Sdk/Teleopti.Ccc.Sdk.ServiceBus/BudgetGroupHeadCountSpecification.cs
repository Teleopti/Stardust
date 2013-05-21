using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "HeadCount")]
    public class BudgetGroupHeadCountSpecification : Specification<IAbsenceRequest>, IBudgetGroupHeadCountSpecification
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (BudgetGroupHeadCountSpecification));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IBudgetDayRepository _budgetDayRepository;
        private readonly ISkillDayRepository _skillDayRepository;
        private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

        public BudgetGroupHeadCountSpecification(ISchedulingResultStateHolder schedulingResultStateHolder,
                                                 IScenarioRepository scenarioRepository,
                                                 IBudgetDayRepository budgetDayRepository,
                                                 ISkillDayRepository skillDayRepository,
                                                 IScheduleProjectionReadOnlyRepository
                                                     scheduleProjectionReadOnlyRepository)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _scenarioRepository = scenarioRepository;
            _budgetDayRepository = budgetDayRepository;
            _skillDayRepository = skillDayRepository;
            _scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
            var defaultScenario = _scenarioRepository.LoadDefaultScenario();
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

                if (!isSkillOpenForDateOnly(currentDay, budgetGroup.SkillCollection))
                    continue;

                var allowance = budgetDay.Allowance;
                var alreadyUsedAllowance =
                    _scheduleProjectionReadOnlyRepository.GetNumberOfAbsencesPerDayAndBudgetGroup(
                        budgetGroup.Id.GetValueOrDefault(), currentDay);

                if (Math.Floor(allowance) <= alreadyUsedAllowance)
                    return false;
            }
            return true;
        }

        private bool isSkillOpenForDateOnly(DateOnly date, IEnumerable<ISkill> skills)
        {
            var scenario = _scenarioRepository.LoadDefaultScenario();
            var skillDays = _skillDayRepository.FindRange(new DateOnlyPeriod(date, date), skills.ToList(), scenario);

            if (skillDays != null && skillDays.Count > 0)
            {
                return skillDays.Any(skillDay => skillDay.OpenForWork.IsOpen);
            }
            return false;
        }
    }
}
