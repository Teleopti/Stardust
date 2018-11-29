using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    [Serializable]
    public class OvertimePreferencesGeneralPersonalSetting : SettingValue
    {
        private Guid? _scheduleTagId;
        private Guid? _skillActivtyId;
        private Guid? _overtimeType;
        private TimePeriod _selectTimePeriod = new TimePeriod(TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        private bool _doNotBreakMaxSeatPerWeek = false;
        private bool _doNotBreakNightlyRest = false;
        private bool _doNotBreakWeeklyRest =  false;
        private bool _availableAgentsOnly = false ;
	    private Guid? _ruleSetBagId;
	    private UseSkills _useSkills = UseSkills.All;

        public void MapTo(IOvertimePreferences overtimePreferences , 
						IEnumerable<IScheduleTag> scheduleTags, 
						IEnumerable<IActivity> activityList,
						IList<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets,
						IList<IRuleSetBag> ruleSetBags )
        {
			overtimePreferences.ScheduleTag = scheduleTags.FirstOrDefault(scheduleTag => _scheduleTagId == scheduleTag.Id) ??
			                                  NullScheduleTag.Instance;

	        foreach (var activity in activityList)
            {
				if (_skillActivtyId == null)
				{
					overtimePreferences.SkillActivity = activity;
					break;
				}

                if (_skillActivtyId == activity.Id )
                    overtimePreferences.SkillActivity = activity ;
            }

            foreach (var overtimeType in multiplicatorDefinitionSets)
            {
	            if (_overtimeType == null)
	            {
		            overtimePreferences.OvertimeType = overtimeType;
		            break;
	            }

	            if (_overtimeType == overtimeType.Id)
                    overtimePreferences.OvertimeType = overtimeType;
            }

	        foreach (var ruleSetBag in ruleSetBags)
	        {
		        if (_ruleSetBagId == null)
		        {
			        overtimePreferences.ShiftBagToUse = null;
			        break;
		        }

		        if (ruleSetBag.Id == _ruleSetBagId)
		        {
			        overtimePreferences.ShiftBagToUse = ruleSetBag;
			        break;
		        }
		        

	        }

            overtimePreferences.SelectedTimePeriod = _selectTimePeriod;
            overtimePreferences.AllowBreakMaxWorkPerWeek = _doNotBreakMaxSeatPerWeek;
            overtimePreferences.AllowBreakNightlyRest = _doNotBreakNightlyRest;
            overtimePreferences.AllowBreakWeeklyRest = _doNotBreakWeeklyRest;
            overtimePreferences.AvailableAgentsOnly = _availableAgentsOnly;
	        overtimePreferences.UseSkills = _useSkills;
        }

        public void MapFrom(IOvertimePreferences overtimePreferences)
        {
            _doNotBreakMaxSeatPerWeek = overtimePreferences.AllowBreakMaxWorkPerWeek;
            _doNotBreakNightlyRest = overtimePreferences.AllowBreakNightlyRest;
            _doNotBreakWeeklyRest = overtimePreferences.AllowBreakWeeklyRest;
            _availableAgentsOnly = overtimePreferences.AvailableAgentsOnly;
            _selectTimePeriod = overtimePreferences.SelectedTimePeriod;
            if (overtimePreferences.OvertimeType!=null)
                _overtimeType = overtimePreferences.OvertimeType.Id;
            if(overtimePreferences.ScheduleTag != null)
                _scheduleTagId = overtimePreferences.ScheduleTag.Id ;
            _skillActivtyId = overtimePreferences.SkillActivity.Id;
	        if (overtimePreferences.ShiftBagToUse != null)
		        _ruleSetBagId = overtimePreferences.ShiftBagToUse.Id;
	        else
	        {
		        _ruleSetBagId = null;
	        }

	        _useSkills = overtimePreferences.UseSkills;
        }
    }
}