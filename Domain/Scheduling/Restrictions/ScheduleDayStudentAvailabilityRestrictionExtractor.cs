using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    public class ScheduleDayStudentAvailabilityRestrictionExtractor : IScheduleDayStudentAvailabilityRestrictionExtractor
    {
        private readonly IRestrictionExtractor _restrictionExtractor;

        public ScheduleDayStudentAvailabilityRestrictionExtractor(IRestrictionExtractor restrictionExtractor)
        {
            _restrictionExtractor = restrictionExtractor;
        }

        public IList<IScheduleDay> AllUnavailable(IList<IScheduleDay> scheduleDays)
        {
            if(scheduleDays == null)
                throw new ArgumentNullException("scheduleDays");

            IList<IScheduleDay> restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                _restrictionExtractor.Extract(scheduleDay);

                if (_restrictionExtractor.StudentAvailabilityList.Count() != 0) continue;

                var personPeriod = scheduleDay.Person.Period(scheduleDay.DateOnlyAsPeriod.DateOnly);

                if(personPeriod != null && personPeriod.PersonContract.Contract.EmploymentType == EmploymentType.HourlyStaff )
                    restrictedDays.Add(scheduleDay);
            }

            return restrictedDays;
        }


        public IList<IScheduleDay> AllAvailable(IList<IScheduleDay> scheduleDays)
        {
            if(scheduleDays == null)
                throw new ArgumentNullException("scheduleDays");

            IList<IScheduleDay> restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                _restrictionExtractor.Extract(scheduleDay);

                if (_restrictionExtractor.StudentAvailabilityList.Count() == 0) continue;

                restrictedDays.Add(scheduleDay);
            }

            return restrictedDays;
        }


        public IScheduleDay RestrictionFulfilled(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay)
        {
            if (restrictionChecker == null)
                throw new ArgumentNullException("restrictionChecker");

            return restrictionChecker.CheckStudentAvailability(scheduleDay) == PermissionState.Satisfied ? scheduleDay : null;
        }
    }
}
