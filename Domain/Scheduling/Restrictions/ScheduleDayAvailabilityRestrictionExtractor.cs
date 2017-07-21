using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    public class ScheduleDayAvailabilityRestrictionExtractor : IScheduleDayAvailabilityRestrictionExtractor
    {
        private readonly IRestrictionExtractor _restrictionExtractor;

        public ScheduleDayAvailabilityRestrictionExtractor(IRestrictionExtractor restrictionExtractor)
        {
            _restrictionExtractor = restrictionExtractor;
        }

        public IList<IScheduleDay> AllUnavailable(IList<IScheduleDay> scheduleDays)
        {
            if(scheduleDays == null)
                throw new ArgumentNullException(nameof(scheduleDays));

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                var result = _restrictionExtractor.Extract(scheduleDay);

                foreach (var availabilityRestriction in result.AvailabilityList)
                {
                    if (availabilityRestriction.NotAvailable && !restrictedDays.Contains(scheduleDay))
                        restrictedDays.Add(scheduleDay);
                }
            }

            return restrictedDays;
        }

        public IList<IScheduleDay> AllAvailable(IList<IScheduleDay> scheduleDays)
        {
            if(scheduleDays == null)
                throw new ArgumentNullException(nameof(scheduleDays));

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                var result = _restrictionExtractor.Extract(scheduleDay);

                foreach (var availabilityRestriction in result.AvailabilityList)
                {
                    if(!availabilityRestriction.NotAvailable && !restrictedDays.Contains(scheduleDay))
                        restrictedDays.Add(scheduleDay);
                }
            }

            return restrictedDays;
        }

        public IScheduleDay RestrictionFulfilled(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay)
        {
            if (restrictionChecker == null)
                throw new ArgumentNullException(nameof(restrictionChecker));

            return restrictionChecker.CheckAvailability(scheduleDay) == PermissionState.Satisfied ? scheduleDay : null;
        }
    }
}
