using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    public class ScheduleDayPreferenceRestrictionExtractor : IScheduleDayPreferenceRestrictionExtractor
    {
        private readonly IRestrictionExtractor _restrictionExtractor;

        public ScheduleDayPreferenceRestrictionExtractor(IRestrictionExtractor restrictionExtractor)
        {
            _restrictionExtractor = restrictionExtractor;
        }

        public IList<IScheduleDay> AllRestrictedDays(IList<IScheduleDay> scheduleDays)
        {
            if(scheduleDays == null)
                throw new ArgumentNullException(nameof(scheduleDays));

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                var result = _restrictionExtractor.Extract(scheduleDay);
  
                if(result.PreferenceList.Any())
                    restrictedDays.Add(scheduleDay);
            }

            return restrictedDays;
        }

        public IList<IScheduleDay> AllRestrictedDaysMustHave(IList<IScheduleDay> scheduleDays)
        {
            if (scheduleDays == null)
                throw new ArgumentNullException(nameof(scheduleDays));

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                var result = _restrictionExtractor.Extract(scheduleDay);

                if (result.PreferenceList.Any(restriction => restriction.MustHave))
                {
                    restrictedDays.Add(scheduleDay);
                }
            }

            return restrictedDays;    
        }

        public IList<IScheduleDay> AllRestrictedAbsences(IList<IScheduleDay> scheduleDays)
        {
            if(scheduleDays == null)
                throw new ArgumentNullException(nameof(scheduleDays));

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                var result = _restrictionExtractor.Extract(scheduleDay);

                foreach (var restriction in result.PreferenceList)
                {
                    if (restriction.Absence != null && !restrictedDays.Contains(scheduleDay))
                        restrictedDays.Add(scheduleDay);
                }
            }

            return restrictedDays;
        }

        public IList<IScheduleDay> AllRestrictedDayOffs(IList<IScheduleDay> scheduleDays)
        {
            if(scheduleDays == null)
                throw new ArgumentNullException(nameof(scheduleDays));

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                var result = _restrictionExtractor.Extract(scheduleDay);

                foreach (var restriction in result.PreferenceList)
                {
                    if(restriction.DayOffTemplate != null && !restrictedDays.Contains(scheduleDay))
                        restrictedDays.Add(scheduleDay);
                }
            }

            return restrictedDays;
        }


        public IList<IScheduleDay> AllRestrictedShifts(IList<IScheduleDay> scheduleDays)
        {
            if(scheduleDays == null)
                throw new ArgumentNullException(nameof(scheduleDays));

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                var result = _restrictionExtractor.Extract(scheduleDay);

                foreach (var restriction in result.PreferenceList)
                {
                    if(restriction.ShiftCategory != null && !restrictedDays.Contains(scheduleDay))
                        restrictedDays.Add(scheduleDay);
                }                
            }

            return restrictedDays;
        }

        public IScheduleDay RestrictionFulfilled(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay)
        {
            if(restrictionChecker == null)
                throw new ArgumentNullException(nameof(restrictionChecker));

            return restrictionChecker.CheckPreference(scheduleDay) == PermissionState.Satisfied ? scheduleDay : null;
        }

		public IScheduleDay RestrictionFulfilledAbsence(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay)
        {
            if(restrictionChecker == null)
                throw new ArgumentNullException(nameof(restrictionChecker));

			return restrictionChecker.CheckPreferenceAbsence(PermissionState.Unspecified, scheduleDay) == PermissionState.Satisfied ? scheduleDay : null;
        }

		public IScheduleDay RestrictionFulfilledDayOff(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay)
        {
            if(restrictionChecker == null)
                throw new ArgumentNullException(nameof(restrictionChecker));

			return restrictionChecker.CheckPreferenceDayOff(scheduleDay) == PermissionState.Satisfied ? scheduleDay : null;
        }

		public IScheduleDay RestrictionFulfilledShift(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay)
        {
           if(restrictionChecker == null)
               throw new ArgumentNullException(nameof(restrictionChecker));

		   return restrictionChecker.CheckPreferenceShift(scheduleDay) == PermissionState.Satisfied ? scheduleDay : null;
        }

        public IScheduleDay RestrictionFulfilledMustHave(ICheckerRestriction restrictionChecker, IScheduleDay scheduleDay)
        {
            if(restrictionChecker == null)
                throw new ArgumentNullException(nameof(restrictionChecker));

            return restrictionChecker.CheckPreferenceMustHave(scheduleDay) == PermissionState.Satisfied ? scheduleDay : null;
        }
    }
}
