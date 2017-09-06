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
            if (scheduleDays == null) throw new ArgumentNullException(nameof(scheduleDays));

			return (from scheduleDay in scheduleDays
				let result = _restrictionExtractor.Extract(scheduleDay)
				where result.RotationList.Any(rotRestriction =>
					rotRestriction.StartTimeLimitation.HasValue() || rotRestriction.EndTimeLimitation.HasValue() ||
					rotRestriction.WorkTimeLimitation.HasValue() || rotRestriction.ShiftCategory != null ||
					rotRestriction.DayOffTemplate != null)
				select scheduleDay).ToList();
		}
		
        public IList<IScheduleDay> AllRestrictedDayOffs(IList<IScheduleDay> scheduleDays)
        {
            if (scheduleDays == null)
                throw new ArgumentNullException(nameof(scheduleDays));

			return (from scheduleDay in scheduleDays
				let result = _restrictionExtractor.Extract(scheduleDay)
				where result.RotationList.Any(r => r.DayOffTemplate != null)
				select scheduleDay).ToList();
        }

        public IList<IScheduleDay> AllRestrictedShifts(IList<IScheduleDay> scheduleDays)
        {
            if (scheduleDays == null)
                throw new ArgumentNullException(nameof(scheduleDays));
			
			return (from scheduleDay in scheduleDays
				let result = _restrictionExtractor.Extract(scheduleDay)
				where result.RotationList.Any(r => r.ShiftCategory != null)
				select scheduleDay).ToList();
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
