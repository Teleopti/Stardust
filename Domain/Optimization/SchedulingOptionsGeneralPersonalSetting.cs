using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

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
        
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void MapTo(ISchedulingOptions schedulingOptions, IEnumerable<IScheduleTag> scheduleTags)
		{
			foreach (var scheduleTag in scheduleTags)
			{
				if (_scheduleTagId == scheduleTag.Id)
					schedulingOptions.TagToUseOnScheduling = scheduleTag;
			}
			if (schedulingOptions.TagToUseOnScheduling == null)
				schedulingOptions.TagToUseOnScheduling = NullScheduleTag.Instance;

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void MapFrom(ISchedulingOptions schedulingOptions)
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