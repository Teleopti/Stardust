using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	[Serializable]
	public class AdvancedPreferencesPersonalSettings : SettingValue
	{
		private TargetValueOptions _targetValueCalculation;
		private bool _useTweakedValues;
		private bool _useMinimumStaffing;
		private bool _useMaximumStaffing;
		private MaxSeatsFeatureOptions _userOptionMaxSeatsFeature;
		private bool _useAverageShiftLengths;
		private int _refreshScreenInterval;
		private int _breakPreferenceStartTimeByMaxHours;

		public AdvancedPreferencesPersonalSettings()
		{
			SetDefaultValues();
		}

		public void MapTo(IAdvancedPreferences target)
		{
		    if (target == null) return;
            target.TargetValueCalculation = _targetValueCalculation;
			target.UseTweakedValues = _useTweakedValues;

			target.UseMinimumStaffing = _useMinimumStaffing;
			target.UseMaximumStaffing = _useMaximumStaffing;
			target.UserOptionMaxSeatsFeature = _userOptionMaxSeatsFeature;
			target.UseAverageShiftLengths = _useAverageShiftLengths;

			target.RefreshScreenInterval = _refreshScreenInterval;
			target.BreakPreferenceStartTimeByMax = TimeSpan.FromHours(_breakPreferenceStartTimeByMaxHours);
		}

		public void MapFrom(IAdvancedPreferences source)
		{
		    if (source == null) return;
            _targetValueCalculation = source.TargetValueCalculation;
			_useTweakedValues = source.UseTweakedValues;

			_useMinimumStaffing = source.UseMinimumStaffing;
			_useMaximumStaffing = source.UseMaximumStaffing;
			_userOptionMaxSeatsFeature = source.UserOptionMaxSeatsFeature;
			_useAverageShiftLengths = source.UseAverageShiftLengths;

			_refreshScreenInterval = source.RefreshScreenInterval;
			_breakPreferenceStartTimeByMaxHours = source.BreakPreferenceStartTimeByMax.Hours;
		}

		private void SetDefaultValues()
		{
			_targetValueCalculation = TargetValueOptions.StandardDeviation;
			_useMinimumStaffing = true;
			_useMaximumStaffing = true;
			_userOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats;
			_useAverageShiftLengths = true;
			_refreshScreenInterval = 10;
			_breakPreferenceStartTimeByMaxHours = 0;
		}
	}
}
