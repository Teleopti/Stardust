using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    public class ScheduleDayRotationRestrictionExtractor : IScheduleDayRotationRestrictionExtractor
    {
        private readonly IRestrictionExtractor _restrictionExtractor;

        public ScheduleDayRotationRestrictionExtractor(IRestrictionExtractor restrictionExtractor)
        {
            _restrictionExtractor = restrictionExtractor;
        }

        public IList<IScheduleDay> AllRestrictedDays(IList<IScheduleDay> scheduleDays)
        {
            if (scheduleDays == null) throw new ArgumentNullException("scheduleDays");

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
	            _restrictionExtractor.Extract(scheduleDay);

	            if (_restrictionExtractor.RotationList.Any(
					rotRestriction => rotRestriction.StartTimeLimitation.HasValue() 
					|| rotRestriction.EndTimeLimitation.HasValue() 
					|| rotRestriction.WorkTimeLimitation.HasValue() 
					|| rotRestriction.ShiftCategory != null 
					|| rotRestriction.DayOffTemplate != null))
	            {
		            restrictedDays.Add(scheduleDay);
	            }
            }

	        return restrictedDays;
        }


        public IList<IScheduleDay> AllRestrictedDayOffs(IList<IScheduleDay> scheduleDays)
        {
            if (scheduleDays == null)
                throw new ArgumentNullException("scheduleDays");

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                _restrictionExtractor.Extract(scheduleDay);

                foreach (var restriction in _restrictionExtractor.RotationList)
                {
                    if(restriction.DayOffTemplate != null && !restrictedDays.Contains(scheduleDay))
                        restrictedDays.Add(scheduleDay);
                }
            }

            return restrictedDays;
        }


        public IList<IScheduleDay> AllRestrictedShifts(IList<IScheduleDay> scheduleDays)
        {
            if (scheduleDays == null)
                throw new ArgumentNullException("scheduleDays");

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                _restrictionExtractor.Extract(scheduleDay);

                foreach (var restriction in _restrictionExtractor.RotationList)
                {
                    if(restriction.ShiftCategory != null && !restrictedDays.Contains(scheduleDay))
                        restrictedDays.Add(scheduleDay);
                }
            }

            return restrictedDays;
        }


        public IScheduleDay RestrictionFulfilled(ICheckerRestriction restrictionChecker)
        {
            if (restrictionChecker == null)
                throw new ArgumentNullException("restrictionChecker");

            return restrictionChecker.CheckRotations() == PermissionState.Satisfied ? restrictionChecker.ScheduleDay : null;
        }


        public IScheduleDay RestrictionFulfilledDayOff(ICheckerRestriction restrictionChecker)
        {
            if(restrictionChecker == null)
                throw new ArgumentNullException("restrictionChecker");

            return restrictionChecker.CheckRotationDayOff() == PermissionState.Satisfied ? restrictionChecker.ScheduleDay : null;
        }


        public IScheduleDay RestrictionFulfilledShift(ICheckerRestriction restrictionChecker)
        {
            if(restrictionChecker == null)
                throw new ArgumentNullException("restrictionChecker");

            return restrictionChecker.CheckRotationShift() == PermissionState.Satisfied ? restrictionChecker.ScheduleDay : null;
        }
    }
}
