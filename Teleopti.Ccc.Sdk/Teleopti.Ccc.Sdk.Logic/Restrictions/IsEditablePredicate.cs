using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
    public class IsEditablePredicate : IIsEditablePredicate
    {
        private readonly Func<DateTime> _currentDateProvider;

        public IsEditablePredicate() : this(() => DateTime.Now.Date) {}

        public IsEditablePredicate(Func<DateTime> currentDateProvider)
        {
            _currentDateProvider = currentDateProvider;
        }

        public bool IsPreferenceEditable(DateOnly calendarDate, IPerson person)
        {
            if (person.WorkflowControlSet == null)
            {
                return false;
            }
            if (!person.WorkflowControlSet.PreferencePeriod.Contains(calendarDate))
            {
                return false;
            }
            if (!person.WorkflowControlSet.PreferenceInputPeriod.Contains(new DateOnly(_currentDateProvider.Invoke().Date)))
            {
                return false;
            }
            return true;
        }

        public bool IsStudentAvailabilityEditable(DateOnly calendarDate, IPerson person)
        {
            if (person.WorkflowControlSet == null)
            {
                return false;
            }
            if (!person.WorkflowControlSet.StudentAvailabilityPeriod.Contains(calendarDate))
            {
                return false;
            }
            if (!person.WorkflowControlSet.StudentAvailabilityInputPeriod.Contains(new DateOnly(_currentDateProvider.Invoke().Date)))
            {
                return false;
            }
            return true;     
        }
    }
}