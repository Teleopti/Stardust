using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
	            var result = _restrictionExtractor.Extract(scheduleDay);

	            if (result.RotationList.Any(
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
                throw new ArgumentNullException(nameof(scheduleDays));

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                var result = _restrictionExtractor.Extract(scheduleDay);

                foreach (var restriction in result.RotationList)
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
                throw new ArgumentNullException(nameof(scheduleDays));

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                var result = _restrictionExtractor.Extract(scheduleDay);

                foreach (var restriction in result.RotationList)
                {
                    if(restriction.ShiftCategory != null && !restrictedDays.Contains(scheduleDay))
                        restrictedDays.Add(scheduleDay);
                }
            }

            return restrictedDays;
        }


        public IScheduleDay RestrictionFulfilled(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay)
        {
            if (restrictionChecker == null)
                throw new ArgumentNullException(nameof(restrictionChecker));

            return restrictionChecker.CheckRotations(scheduleDay) == PermissionState.Satisfied ? scheduleDay : null;
        }


		public IScheduleDay RestrictionFulfilledDayOff(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay)
        {
            if(restrictionChecker == null)
                throw new ArgumentNullException(nameof(restrictionChecker));

			return restrictionChecker.CheckRotationDayOff(scheduleDay) == PermissionState.Satisfied ? scheduleDay : null;
        }


		public IScheduleDay RestrictionFulfilledShift(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay)
        {
            if(restrictionChecker == null)
                throw new ArgumentNullException(nameof(restrictionChecker));

			return restrictionChecker.CheckRotationShift(scheduleDay) == PermissionState.Satisfied ? scheduleDay : null;
        }
    }
}
