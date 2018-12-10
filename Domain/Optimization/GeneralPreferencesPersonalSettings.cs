using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class GeneralPreferencesPersonalSettings : SettingValue
	{
		private Guid? _scheduleTagId;

		private bool _optimizationStepDaysOff;
		private bool _optimizationStepTimeBetweenDays;
		private bool _optimizationStepShiftsWithinDay;
		private bool _optimizationStepShiftsForFlexibleWorkTime;
		private bool _optimizationStepDaysOffForFlexibleWorkTime;
		private bool _optimizationStepFairness;
		private bool _optimizationStepIntraInterval;

		private bool _usePreferences;
		private bool _useMustHaves;
		private bool _useRotations;
		private bool _useAvailabilities;
		private bool _useStudentAvailabilities;
		private bool _useShiftCategoryLimitations;

		private double _preferencesValue;
		private double _mustHavesValue;
		private double _rotationsValue;
		private double _availabilitiesValue;
		private double _studentAvailabilitiesValue;

		public GeneralPreferencesPersonalSettings()
		{
			SetDefaultValues();
		}

		public void MapTo(GeneralPreferences target, IEnumerable<IScheduleTag> scheduleTags)
		{
			InParameter.NotNull(nameof(scheduleTags), scheduleTags);

			foreach (var scheduleTag in scheduleTags)
			{
				if (_scheduleTagId == scheduleTag.Id)
					target.ScheduleTag = scheduleTag;
			}

			if (target.ScheduleTag == null)
				target.ScheduleTag = NullScheduleTag.Instance;

			target.OptimizationStepDaysOff = _optimizationStepDaysOff;
			target.OptimizationStepTimeBetweenDays = _optimizationStepTimeBetweenDays;
			target.OptimizationStepShiftsWithinDay = _optimizationStepShiftsWithinDay;
			target.OptimizationStepShiftsForFlexibleWorkTime = _optimizationStepShiftsForFlexibleWorkTime;
			target.OptimizationStepDaysOffForFlexibleWorkTime = _optimizationStepDaysOffForFlexibleWorkTime;
			target.OptimizationStepFairness = _optimizationStepFairness;
			target.OptimizationStepIntraInterval = _optimizationStepIntraInterval;

			target.UsePreferences = _usePreferences;
			target.UseMustHaves = _useMustHaves;
			target.UseRotations = _useRotations;
			target.UseAvailabilities = _useAvailabilities;
			target.UseStudentAvailabilities = _useStudentAvailabilities;
			target.UseShiftCategoryLimitations = _useShiftCategoryLimitations;

			target.PreferencesValue = _preferencesValue;
			target.MustHavesValue = _mustHavesValue;
			target.RotationsValue = _rotationsValue;
			target.AvailabilitiesValue = _availabilitiesValue;
			target.StudentAvailabilitiesValue = _studentAvailabilitiesValue;

		}

		public void MapFrom(GeneralPreferences source)
		{
			if (source.ScheduleTag != null)
				_scheduleTagId = source.ScheduleTag.Id;

			_optimizationStepDaysOff = source.OptimizationStepDaysOff;
			_optimizationStepTimeBetweenDays = source.OptimizationStepTimeBetweenDays;
			_optimizationStepShiftsWithinDay = source.OptimizationStepShiftsWithinDay;
			_optimizationStepShiftsForFlexibleWorkTime = source.OptimizationStepShiftsForFlexibleWorkTime;
			_optimizationStepDaysOffForFlexibleWorkTime = source.OptimizationStepDaysOffForFlexibleWorkTime;
			_optimizationStepFairness = source.OptimizationStepFairness;
			_optimizationStepIntraInterval = source.OptimizationStepIntraInterval;

			_usePreferences = source.UsePreferences;
			_useMustHaves = source.UseMustHaves;
			_useRotations = source.UseRotations;
			_useAvailabilities = source.UseAvailabilities;
			_useStudentAvailabilities = source.UseStudentAvailabilities;
			_useShiftCategoryLimitations = source.UseShiftCategoryLimitations;

			_preferencesValue = source.PreferencesValue;
			_mustHavesValue = source.MustHavesValue;
			_rotationsValue = source.RotationsValue;
			_availabilitiesValue = source.AvailabilitiesValue;
			_studentAvailabilitiesValue = source.StudentAvailabilitiesValue;
		}

		/// <summary>
		/// Sets the schedule tag id.
		/// </summary>
		/// <param name="guid">The GUID.</param>
		/// <remarks>Used in tests only</remarks>
		public void SetScheduleTagId(Guid guid)
		{
			_scheduleTagId = guid;
		}

		private void SetDefaultValues()
		{
			_optimizationStepDaysOff = true;
			_optimizationStepTimeBetweenDays = true;
			_optimizationStepShiftsWithinDay = true;
			_optimizationStepIntraInterval = false;

			_usePreferences = true;
			_useMustHaves = true;
			_useRotations = true;
			_useAvailabilities = true;
			_useStudentAvailabilities = false;
			_useShiftCategoryLimitations = true;

			_preferencesValue = 0.8d;
			_mustHavesValue = 1d;
			_rotationsValue = 1d;
			_availabilitiesValue = 1d;
			_studentAvailabilitiesValue = 1d;
		}

	}
}
