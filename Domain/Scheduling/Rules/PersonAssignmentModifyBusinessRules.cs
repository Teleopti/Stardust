using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    /// <summary>
    /// Holds businessrules that are checked only on when a part is modified
    /// </summary>
    public class PersonAssignmentModifyBusinessRules : AggregateRootBusinessRules
    {
        public PersonAssignmentModifyBusinessRules(ISchedulePart modifiedPart)
        {
            BusinessRules.Add(new PersonAccountRule(modifiedPart));
        }

        public PersonAssignmentModifyBusinessRules(ISchedulePart modifiedPart, ISchedulingResultStateHolder stateHolder)
        {
            if (stateHolder != null && stateHolder.Schedules.ContainsKey(modifiedPart.Person))
                BusinessRules.Add(new PersonAccountRule(modifiedPart,stateHolder));//stateHolder.Schedules[modifiedPart.Person]));
        }

        public PersonAssignmentModifyBusinessRules(ISchedulePart modifiedPart, ISchedulingResultStateHolder stateHolder,ISchedule visibleSchedule)
        {
            if (stateHolder != null && stateHolder.Schedules.ContainsKey(modifiedPart.Person))
            {
                PersonAccountRule rule = new PersonAccountRule(modifiedPart, stateHolder, visibleSchedule);
                BusinessRules.Add(rule);
            }
        }
    }
}
