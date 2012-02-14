using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    /// <summary>
    /// Business rules for the person assignment entity
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-08
    /// </remarks>
    public sealed class PersonAssignmentBusinessRules : BusinessRuleCollection
    {
        public PersonAssignmentBusinessRules(ISchedulePart schedulePart, ISchedulingResultStateHolder schedulingResultStateHolder,
            IPersonAssignmentBusinessRulesOptions personAssignmentBusinessRulesOptions)
        {
            //Add more rules for PersonAssignment here!
            BusinessRules.Add(new NightlyRestRule());
            BusinessRules.Add(new OverlappingAssignmentRule());
            BusinessRules.Add(new DayOffRule());
            BusinessRules.Add(new MaxWeekWorkTimeRule());
            BusinessRules.Add(new OpenHoursRule(schedulingResultStateHolder));
            BusinessRules.Add(new LegalStateRule(schedulingResultStateHolder, personAssignmentBusinessRulesOptions));
            BusinessRules.Add(new MaxOneDayOffRule());
            BusinessRules.Add(new LayerOwnerNeedsAtLeastOneLayerRule());
//            BusinessRules.Add(new PersonAccountRule(schedulingResultStateHolder.Schedules[schedulePart.Person],schedulingResultStateHolder.PersonAccountProvider, schedulePart));
        }
    }
}