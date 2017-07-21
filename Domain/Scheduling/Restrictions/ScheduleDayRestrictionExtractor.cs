using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    public class ScheduleDayRestrictionExtractor : IScheduleDayRestrictionExtractor
    {
        private readonly IRestrictionExtractor _restrictionExtractor;

        public ScheduleDayRestrictionExtractor(IRestrictionExtractor restrictionExtractor)
        {
            _restrictionExtractor = restrictionExtractor;
        }

        public IList<IScheduleDay> AllRestrictedDays(IList<IScheduleDay> scheduleDays)
        {
            if(scheduleDays == null) throw new ArgumentNullException(nameof(scheduleDays));

            IList<IScheduleDay> restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                var result = _restrictionExtractor.Extract(scheduleDay);

                if(result.PreferenceList.Any() || result.RotationList.Any() || result.StudentAvailabilityList.Any() || result.AvailabilityList.Any())
                    restrictedDays.Add(scheduleDay);  
            }

            return restrictedDays;
        }
    }
}
