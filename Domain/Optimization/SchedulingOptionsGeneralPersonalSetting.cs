using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class SchedulingOptionsGeneralPersonalSetting : SettingValue
	{
        private bool _rotationDaysOnly;
		private bool _useAvailability = true;
		private bool _availabilityDaysOnly;
        private bool _useStudentAvailability;
		private bool _usePreferences = true;
		private bool _preferenceDaysOnly;
		private bool _useMustHavesOnly;
		
		private bool _useShiftCategoryLimitations = true;
		private Guid? _scheduleTagId;
        private bool _useRotations = true;
		private bool _showTroubleshotInformation;
        
		public void MapTo(SchedulingOptions schedulingOptions, IEnumerable<IScheduleTag> scheduleTags)
		{
			schedulingOptions.TagToUseOnScheduling = scheduleTags.FirstOrDefault(scheduleTag => _scheduleTagId == scheduleTag.Id) ??
			                                         NullScheduleTag.Instance;

			schedulingOptions.UseRotations = _useRotations;
            schedulingOptions.RotationDaysOnly = _rotationDaysOnly;
			schedulingOptions.UseAvailability = _useAvailability;
			schedulingOptions.AvailabilityDaysOnly = _availabilityDaysOnly;
            
			schedulingOptions.UseStudentAvailability = _useStudentAvailability;
			schedulingOptions.UsePreferences = _usePreferences;
			schedulingOptions.PreferencesDaysOnly = _preferenceDaysOnly;
			schedulingOptions.UsePreferencesMustHaveOnly = _useMustHavesOnly;
			schedulingOptions.UseShiftCategoryLimitations = _useShiftCategoryLimitations;
			schedulingOptions.ShowTroubleshot = _showTroubleshotInformation;
		}

		public void MapFrom(SchedulingOptions schedulingOptions)
		{
			_scheduleTagId = schedulingOptions.TagToUseOnScheduling.Id;
			_useRotations = schedulingOptions.UseRotations;
            _rotationDaysOnly = schedulingOptions.RotationDaysOnly;
			_useAvailability = schedulingOptions.UseAvailability;
			_availabilityDaysOnly = schedulingOptions.AvailabilityDaysOnly;
           _useStudentAvailability = schedulingOptions.UseStudentAvailability;
			_usePreferences = schedulingOptions.UsePreferences;
			_preferenceDaysOnly = schedulingOptions.PreferencesDaysOnly;
			_useMustHavesOnly = schedulingOptions.UsePreferencesMustHaveOnly;
			_useShiftCategoryLimitations = schedulingOptions.UseShiftCategoryLimitations;
			_showTroubleshotInformation = schedulingOptions.ShowTroubleshot;
		}
	}
}