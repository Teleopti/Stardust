using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    [Serializable]
    public  class OvertimePreferencesGeneralPersonalSetting : SettingValue
    {
        private Guid? _scheduleTagId;
        private bool _extendExistingShifts = true;
        private Guid? _skillActivtyId;
        private Guid? _overtimeType;
        private TimePeriod _selectTimePeriod;
        private bool _doNotBreakMaxSeatPerWeek = false;
        private bool _doNotBreakNightlyRest = false;
        private bool _doNotBreakWeeklyRest =  false;

        public void MapTo(IOvertimePreferences overtimePreferences , IList<IScheduleTag> scheduleTags,IList<IActivity> activityList,IList<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets  )
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

            foreach (var overtimeType in multiplicatorDefinitionSets)
            {
                if (_overtimeType == overtimeType.Id)
                    overtimePreferences.OvertimeType = overtimeType;
            }

            overtimePreferences.ExtendExistingShift = _extendExistingShifts;
            overtimePreferences.SelectedTimePeriod = _selectTimePeriod;
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
            
            _selectTimePeriod = overtimePreferences.SelectedTimePeriod;
            if (overtimePreferences.OvertimeType!=null)
                _overtimeType = overtimePreferences.OvertimeType.Id;
            if(overtimePreferences.ScheduleTag != null)
                _scheduleTagId = overtimePreferences.ScheduleTag.Id ;
            _skillActivtyId = overtimePreferences.SkillActivity.Id;
        }
    }
}