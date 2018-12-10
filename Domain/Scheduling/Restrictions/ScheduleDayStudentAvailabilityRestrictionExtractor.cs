using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
                throw new ArgumentNullException(nameof(scheduleDays));

            IList<IScheduleDay> restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                var result = _restrictionExtractor.Extract(scheduleDay);

                if (result.StudentAvailabilityList.Any()) continue;

                var personPeriod = scheduleDay.Person.Period(scheduleDay.DateOnlyAsPeriod.DateOnly);

                if(personPeriod != null && personPeriod.PersonContract.Contract.EmploymentType == EmploymentType.HourlyStaff )
                    restrictedDays.Add(scheduleDay);
            }

            return restrictedDays;
        }


        public IList<IScheduleDay> AllAvailable(IList<IScheduleDay> scheduleDays)
        {
            if(scheduleDays == null)
                throw new ArgumentNullException(nameof(scheduleDays));

	        List<IScheduleDay> list = new List<IScheduleDay>();
	        foreach (var scheduleDay in scheduleDays)
	        {
		        IExtractedRestrictionResult result = _restrictionExtractor.Extract(scheduleDay);
		        if (result.StudentAvailabilityList.Any()) list.Add(scheduleDay);
	        }
	        return list;
        }


        public IScheduleDay RestrictionFulfilled(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay)
        {
            if (restrictionChecker == null)
                throw new ArgumentNullException(nameof(restrictionChecker));

            return restrictionChecker.CheckStudentAvailability(scheduleDay) == PermissionState.Satisfied ? scheduleDay : null;
        }
    }
}
