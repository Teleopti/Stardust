using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class PreSchedulingStatusChecker : IPreSchedulingStatusChecker
    {
        private DateTime _scheduleDayUtc;
        private DateOnly _scheduleDateOnly;
        private DateTimePeriod _validPeriod;
        private IPersonPeriod _currentPersonPeriod;
        private IVirtualSchedulePeriod _currentSchedulePeriod;
        private IScheduleDay _schedulePart;

        public bool CheckStatus(IScheduleDay schedulePart, SchedulingOptions schedulingOptions)
        {
            _schedulePart = schedulePart;
            _validPeriod = schedulePart.DateOnlyAsPeriod.Period();
            _scheduleDayUtc = _validPeriod.StartDateTime;

            _scheduleDateOnly = schedulePart.DateOnlyAsPeriod.DateOnly;
           
            _currentSchedulePeriod = schedulePart.Person.VirtualSchedulePeriod(_scheduleDateOnly);
            if (_currentSchedulePeriod != null && _currentSchedulePeriod.IsValid)
                _currentPersonPeriod = schedulePart.Person.Period(_scheduleDateOnly);

            return checkTheStatus(schedulingOptions);
        }

        private bool checkTheStatus(SchedulingOptions schedulingOptions)
        {
            if (SchedulePeriod.IsValid == false)
            {
                return false;
            }

            if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff && PersonPeriod.PersonContract.Contract.EmploymentType == EmploymentType.HourlyStaff)
            {
                return false;
            }
            //only fixed staff will be scheduled this way
            if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.HourlyStaff && PersonPeriod.PersonContract.Contract.EmploymentType != EmploymentType.HourlyStaff)
            {
                return false;
            }

            //no day off
            if (_schedulePart.IsScheduled())
            {
                return false;
            }

            if (PersonPeriod.RuleSetBag == null)
            {
                return false;
            }
            return true;
        }

        public IPersonPeriod PersonPeriod
        {
            get
            {
                return _currentPersonPeriod;
            }
        }

        public IVirtualSchedulePeriod SchedulePeriod
        {
            get { return _currentSchedulePeriod; }
        }

        public DateTime ScheduleDayUtc
        {
            get { return _scheduleDayUtc; }
        }

        public DateOnly ScheduleDateOnly
        {
            get { return _scheduleDateOnly; }
        }

        public IPerson Person
        {
            get { return _schedulePart.Person; }
        }
    }
}
