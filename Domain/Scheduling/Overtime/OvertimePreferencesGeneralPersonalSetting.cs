using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public  class OvertimePreferencesGeneralPersonalSetting : SettingValue
    {
        private Guid? _scheduleTagId;
        private bool _extendExistingShifts;
        private Guid? _skillActivtyId;
        private Guid? _overtimeType;
        private TimeSpan _overtimeFrom;
        private TimeSpan _overtimeTo;
        private bool _doNotBreakMaxSeatPerWeek;
        private bool _doNotBreakNightlyRest;
        private bool _doNotBreakWeeklyRest;

        public void MapTo(IOvertimePreferences overtimePreferences , IList<IScheduleTag> scheduleTags,IList<IActivity> activityList )
        {
            foreach (var scheduleTag in scheduleTags)
            {
                if (_scheduleTagId == scheduleTag.Id)
                    overtimePreferences.ScheduleTag = scheduleTag;
            }
            if (overtimePreferences.ScheduleTag == null)
                overtimePreferences.ScheduleTag = NullScheduleTag.Instance;
            foreach (var activity in activityList)
            {
                if (_skillActivtyId == activity.Id )
                    overtimePreferences.SkillActivity = activity ;
            }
            if (overtimePreferences.SkillActivity  == null)
                overtimePreferences.SkillActivity  = null ;

            overtimePreferences.ExtendExistingShift = _extendExistingShifts;
            overtimePreferences.OvertimeFrom = _overtimeFrom;
            overtimePreferences.OvertimeTo = _overtimeTo;
            overtimePreferences.DoNotBreakMaxWorkPerWeek = _doNotBreakMaxSeatPerWeek;
            overtimePreferences.DoNotBreakNightlyRest = _doNotBreakNightlyRest;
            overtimePreferences.DoNotBreakWeeklyRest = _doNotBreakWeeklyRest;
            
        }

        public void MapFrom(IOvertimePreferences overtimePreferences)
        {
            _doNotBreakMaxSeatPerWeek = overtimePreferences.DoNotBreakMaxWorkPerWeek;
            _doNotBreakNightlyRest = overtimePreferences.DoNotBreakNightlyRest;
            _doNotBreakWeeklyRest = overtimePreferences.DoNotBreakWeeklyRest;
            _extendExistingShifts = overtimePreferences.ExtendExistingShift;
            _overtimeFrom = overtimePreferences.OvertimeFrom;
            _overtimeTo = overtimePreferences.OvertimeTo;
            _overtimeType = overtimePreferences.OvertimeType;
            _scheduleTagId = overtimePreferences.ScheduleTag.Id ;
            _skillActivtyId = overtimePreferences.SkillActivity.Id;
        }
    }
}