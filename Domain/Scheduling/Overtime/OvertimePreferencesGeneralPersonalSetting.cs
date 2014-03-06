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
        private bool _availableAgentsOnly ;

        public void MapTo(IOvertimePreferences overtimePreferences , IEnumerable<IScheduleTag> scheduleTags,IEnumerable<IActivity> activityList,IList<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets  )
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
            overtimePreferences.AllowBreakMaxWorkPerWeek = _doNotBreakMaxSeatPerWeek;
            overtimePreferences.AllowBreakNightlyRest = _doNotBreakNightlyRest;
            overtimePreferences.AllowBreakWeeklyRest = _doNotBreakWeeklyRest;
            overtimePreferences.AvailableAgentsOnly = _availableAgentsOnly;
        }

        public void MapFrom(IOvertimePreferences overtimePreferences)
        {
            _doNotBreakMaxSeatPerWeek = overtimePreferences.AllowBreakMaxWorkPerWeek;
            _doNotBreakNightlyRest = overtimePreferences.AllowBreakNightlyRest;
            _doNotBreakWeeklyRest = overtimePreferences.AllowBreakWeeklyRest;
            _extendExistingShifts = overtimePreferences.ExtendExistingShift;
            _availableAgentsOnly = overtimePreferences.AvailableAgentsOnly;
            _selectTimePeriod = overtimePreferences.SelectedTimePeriod;
            if (overtimePreferences.OvertimeType!=null)
                _overtimeType = overtimePreferences.OvertimeType.Id;
            if(overtimePreferences.ScheduleTag != null)
                _scheduleTagId = overtimePreferences.ScheduleTag.Id ;
            _skillActivtyId = overtimePreferences.SkillActivity.Id;
        }
    }
}