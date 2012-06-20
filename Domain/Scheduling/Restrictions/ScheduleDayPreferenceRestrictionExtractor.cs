using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

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
                throw new ArgumentNullException("scheduleDays");

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                _restrictionExtractor.Extract(scheduleDay);
  
                if(_restrictionExtractor.PreferenceList.Any())
                    restrictedDays.Add(scheduleDay);
            }

            return restrictedDays;
        }

        public IList<IScheduleDay> AllRestrictedDaysMustHave(IList<IScheduleDay> scheduleDays)
        {
            if (scheduleDays == null)
                throw new ArgumentNullException("scheduleDays");

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                _restrictionExtractor.Extract(scheduleDay);

                if (_restrictionExtractor.PreferenceList.Any(restriction => restriction.MustHave))
                {
                    restrictedDays.Add(scheduleDay);
                }
            }

            return restrictedDays;    
        }

        public IList<IScheduleDay> AllRestrictedAbsences(IList<IScheduleDay> scheduleDays)
        {
            if(scheduleDays == null)
                throw new ArgumentNullException("scheduleDays");

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                _restrictionExtractor.Extract(scheduleDay);

                foreach (var restriction in _restrictionExtractor.PreferenceList)
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
                throw new ArgumentNullException("scheduleDays");

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                _restrictionExtractor.Extract(scheduleDay);

                foreach (var restriction in _restrictionExtractor.PreferenceList)
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
                throw new ArgumentNullException("scheduleDays");

            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                _restrictionExtractor.Extract(scheduleDay);

                foreach (var restriction in _restrictionExtractor.PreferenceList)
                {
                    if(restriction.ShiftCategory != null && !restrictedDays.Contains(scheduleDay))
                        restrictedDays.Add(scheduleDay);
                }                
            }

            return restrictedDays;
        }


        public IScheduleDay RestrictionFulfilled(ICheckerRestriction restrictionChecker)
        {
            if(restrictionChecker == null)
                throw new ArgumentNullException("restrictionChecker");

            return restrictionChecker.CheckPreference() == PermissionState.Satisfied ? restrictionChecker.ScheduleDay : null;
        }


        public IScheduleDay RestrictionFulfilledAbsence(ICheckerRestriction restrictionChecker)
        {
            if(restrictionChecker == null)
                throw new ArgumentNullException("restrictionChecker");

            return restrictionChecker.CheckPreferenceAbsence(PermissionState.Unspecified) == PermissionState.Satisfied ? restrictionChecker.ScheduleDay : null;
        }

        public IScheduleDay RestrictionFulfilledDayOff(ICheckerRestriction restrictionChecker)
        {
            if(restrictionChecker == null)
                throw new ArgumentNullException("restrictionChecker");

            return restrictionChecker.CheckPreferenceDayOff() == PermissionState.Satisfied ? restrictionChecker.ScheduleDay : null;
        }


        public IScheduleDay RestrictionFulfilledShift(ICheckerRestriction restrictionChecker)
        {
           if(restrictionChecker == null)
               throw new ArgumentNullException("restrictionChecker");

            return restrictionChecker.CheckPreferenceShift() == PermissionState.Satisfied ? restrictionChecker.ScheduleDay : null;
        }

        public IScheduleDay RestrictionFulfilledMustHave(ICheckerRestriction restrictionChecker)
        {
            if(restrictionChecker == null)
                throw new ArgumentNullException("restrictionChecker");

            return restrictionChecker.CheckPreferenceMustHave() == PermissionState.Satisfied ? restrictionChecker.ScheduleDay : null;
        }
    }
}
