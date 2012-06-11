using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

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
            if(scheduleDays == null)
                throw new ArgumentNullException("scheduleDays");

            IList<IScheduleDay> restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                _restrictionExtractor.Extract(scheduleDay);

                if(_restrictionExtractor.PreferenceList.Any() || _restrictionExtractor.RotationList.Any() || _restrictionExtractor.StudentAvailabilityList.Any() || _restrictionExtractor.AvailabilityList.Any())
                    restrictedDays.Add(scheduleDay);  
            }

            return restrictedDays;
        }
    }
}
