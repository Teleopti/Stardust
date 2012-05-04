using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class GeneralPreferencesPersonalSettings : SettingValue, IPersonalSettings<IGeneralPreferences>
	{
		private IList<IScheduleTag> ScheduleTags { get; set; }

		private Guid? ScheduleTagId { get; set; }

		private bool OptimizationStepDaysOff { get; set; }
		private bool OptimizationStepTimeBetweenDays { get; set; }
		private bool OptimizationStepShiftsWithinDay { get; set; }
		private bool OptimizationStepShiftsForFlexibleWorkTime { get; set; }
		private bool OptimizationStepDaysOffForFlexibleWorkTime { get; set; }
		
		private bool UsePreferences { get; set; }
		private bool UseMustHaves { get; set; }
		private bool UseRotations { get; set; }
		private bool UseAvailabilities { get; set; }
		private bool UseStudentAvailabilities { get; set; }
		private bool UseShiftCategoryLimitations { get; set; }

		private double PreferencesValue { get; set; }
		private double MustHavesValue { get; set; }
		private double RotationsValue { get; set; }
		private double AvailabilitiesValue { get; set; }
		private double StudentAvailabilitiesValue { get; set; }

		public GeneralPreferencesPersonalSettings(IList<IScheduleTag> scheduleTags)
		{
			// add initial values
			ScheduleTags = scheduleTags;
		}

		public void MapTo(IGeneralPreferences target)
		{
			foreach (var scheduleTag in ScheduleTags)
			{
				if (ScheduleTagId == scheduleTag.Id)
					target.ScheduleTag = scheduleTag;
			}

			target.OptimizationStepDaysOff = OptimizationStepDaysOff;
			target.OptimizationStepTimeBetweenDays = OptimizationStepTimeBetweenDays;
			target.OptimizationStepShiftsWithinDay = OptimizationStepShiftsWithinDay;
			target.OptimizationStepShiftsForFlexibleWorkTime = OptimizationStepShiftsForFlexibleWorkTime;
			target.OptimizationStepDaysOffForFlexibleWorkTime = OptimizationStepDaysOffForFlexibleWorkTime;

			target.UsePreferences = UsePreferences;
			target.UseMustHaves = UseMustHaves;
			target.UseRotations = UseRotations;
			target.UseAvailabilities = UseAvailabilities;
			target.UseStudentAvailabilities = UseStudentAvailabilities;
			target.UseShiftCategoryLimitations = UseShiftCategoryLimitations;

			target.PreferencesValue = PreferencesValue;
			target.MustHavesValue = MustHavesValue;
			target.RotationsValue = RotationsValue;
			target.AvailabilitiesValue = AvailabilitiesValue;
			target.StudentAvailabilitiesValue = StudentAvailabilitiesValue;

		}

		public void MapFrom(IGeneralPreferences source)
		{
			ScheduleTagId = source.ScheduleTag.Id;

			OptimizationStepDaysOff = source.OptimizationStepDaysOff;
			OptimizationStepTimeBetweenDays = source.OptimizationStepTimeBetweenDays;
			OptimizationStepShiftsWithinDay = source.OptimizationStepShiftsWithinDay;
			OptimizationStepShiftsForFlexibleWorkTime = source.OptimizationStepShiftsForFlexibleWorkTime;
			OptimizationStepDaysOffForFlexibleWorkTime = source.OptimizationStepDaysOffForFlexibleWorkTime;

			UsePreferences = source.UsePreferences;
			UseMustHaves = source.UseMustHaves;
			UseRotations = source.UseRotations;
			UseAvailabilities = source.UseAvailabilities;
			UseStudentAvailabilities = source.UseStudentAvailabilities;
			UseShiftCategoryLimitations = source.UseShiftCategoryLimitations;

			PreferencesValue = source.PreferencesValue;
			MustHavesValue = source.MustHavesValue;
			RotationsValue = source.RotationsValue;
			AvailabilitiesValue = source.AvailabilitiesValue;
			StudentAvailabilitiesValue = source.StudentAvailabilitiesValue;
		}

		/// <summary>
		/// Sets the schedule tag id.
		/// </summary>
		/// <param name="guid">The GUID.</param>
		/// <remarks>Used in tests only</remarks>
		public void SetScheduleTagId(Guid guid)
		{
			ScheduleTagId = guid;
		}

	}
}
