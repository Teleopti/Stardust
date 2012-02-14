using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class PreSchedulingStatusChecker : IPreSchedulingStatusChecker
    {
        private readonly ISchedulingOptions _schedulingOptions;
        private DateTime _scheduleDayUtc;
        private DateOnly _scheduleDateOnly;
        private DateTimePeriod _validPeriod;
        private IPersonPeriod _currentPersonPeriod;
        private IVirtualSchedulePeriod _currentSchedulePeriod;
        private IScheduleDay _schedulePart;
        private IWorkShiftFinderResult _finderResult;

        public PreSchedulingStatusChecker(ISchedulingOptions schedulingOptions)
        {
            _schedulingOptions = schedulingOptions;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool CheckStatus(IScheduleDay schedulePart, IWorkShiftFinderResult finderResult)
        {
            _schedulePart = schedulePart;
            _finderResult = finderResult;
            _validPeriod = schedulePart.DateOnlyAsPeriod.Period();
            _scheduleDayUtc = _validPeriod.StartDateTime;

            //ICccTimeZoneInfo timeZoneInfo = Person.PermissionInformation.DefaultTimeZone();
            _scheduleDateOnly = schedulePart.DateOnlyAsPeriod.DateOnly;
           
            _currentSchedulePeriod = schedulePart.Person.VirtualSchedulePeriod(_scheduleDateOnly);
            if (_currentSchedulePeriod != null && _currentSchedulePeriod.IsValid)
                _currentPersonPeriod = schedulePart.Person.Period(_scheduleDateOnly);

            return CheckTheStatus();
        }

        private bool CheckTheStatus()
        {
            if (SchedulePeriod.IsValid == false)
            {
                LoggFilterResult(UserTexts.Resources.NoSchedulePeriodIsDefinedForTheDate, 0, 0);
                return false;
            }

            if (_schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff && PersonPeriod.PersonContract.Contract.EmploymentType == EmploymentType.HourlyStaff)
            {
                LoggFilterResult(UserTexts.Resources.TheEmploymentTypeIsNotFixedStaff, 0, 0);
                return false;
            }
            //only fixed staff will be scheduled this way
            if (_schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.HourlyStaff && PersonPeriod.PersonContract.Contract.EmploymentType != EmploymentType.HourlyStaff)
            {
                LoggFilterResult(UserTexts.Resources.TheEmploymentTypeIsNotHourlyStaff, 0, 0);
                return false;
            }
            //no person assignment
            if (!CheckAssignments(_schedulePart))
            {
                //loggFilterResult(UserTexts.Resources.ThereIsAlreadyAnAssignment, 0, 0);
                return false;
            }
            //no day off
            if (_schedulePart.PersonDayOffCollection().Count != 0)
            {
                LoggFilterResult(UserTexts.Resources.ThereIsAlreadyADayOff, 0, 0);
                return false;
            }
            if (PersonPeriod.RuleSetBag == null)
            {
                LoggFilterResult(UserTexts.Resources.NoRuleSetBagDefined, 0, 0);
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

        public static bool CheckAssignments(ISchedulePart schedulePart)
        {
            //no assignment is ok
            var personAssignmentCollection = schedulePart.PersonAssignmentCollection();
            if (personAssignmentCollection.Count == 0)
                return true;
            //more than 1 is not ok
            //if (personAssignmentCollection.Count > 1)
            //    return false;
            //1 assignment is ok if we have no mainshift and we have a personalshift
            IPersonAssignment personAssignment = personAssignmentCollection[0];

            if (personAssignment.PersonalShiftCollection.Count > 0 && personAssignment.MainShift == null)
                return true;

            return false;
        }

        private void LoggFilterResult(string message, int countWorkShiftsBefore, int countWorkShiftsAfter)
        {
			_finderResult.AddFilterResults(new WorkShiftFilterResult(message, countWorkShiftsBefore, countWorkShiftsAfter));
            //Console.WriteLine(message);
        }
    }
}
