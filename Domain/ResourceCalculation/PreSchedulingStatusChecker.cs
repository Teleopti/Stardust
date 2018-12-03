using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class PreSchedulingStatusChecker
    {
        private DateOnly _scheduleDateOnly;
        private IPersonPeriod _currentPersonPeriod;
        private IVirtualSchedulePeriod _currentSchedulePeriod;
        private IScheduleDay _schedulePart;

        public bool CheckStatus(IScheduleDay schedulePart, SchedulingOptions schedulingOptions)
        {
            _schedulePart = schedulePart;

            _scheduleDateOnly = schedulePart.DateOnlyAsPeriod.DateOnly;
           
            _currentSchedulePeriod = schedulePart.Person.VirtualSchedulePeriod(_scheduleDateOnly);
            if (_currentSchedulePeriod != null && _currentSchedulePeriod.IsValid)
                _currentPersonPeriod = schedulePart.Person.Period(_scheduleDateOnly);

            return checkTheStatus(schedulingOptions);
        }

        private bool checkTheStatus(SchedulingOptions schedulingOptions)
        {
            if (_currentSchedulePeriod.IsValid == false)
            {
                return false;
            }

            if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff && _currentPersonPeriod.PersonContract.Contract.EmploymentType == EmploymentType.HourlyStaff)
            {
                return false;
            }
            //only fixed staff will be scheduled this way
            if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.HourlyStaff && _currentPersonPeriod.PersonContract.Contract.EmploymentType != EmploymentType.HourlyStaff)
            {
                return false;
            }

            //no day off
            if (_schedulePart.IsScheduled())
            {
                return false;
            }

            if (_currentPersonPeriod.RuleSetBag == null)
            {
                return false;
            }
            return true;
        }
    }
}
