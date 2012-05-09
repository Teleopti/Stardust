using System;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class AdvancedPreferencesPersonalSettings : SettingValue
	{
		private TargetValueOptions _targetValueCalculation;
		private bool _useIntraIntervalDeviation;
		private bool _useTweakedValues;
		private bool _useMinimumStaffing;
		private bool _useMaximumStaffing;
		private bool _useMaximumSeats;
		private bool _doNotBreakMaximumSeats;
		private int _refreshScreenInterval;

		public AdvancedPreferencesPersonalSettings()
		{
			SetDefaultValues();
		}

		public void MapTo(IAdvancedPreferences target)
		{
			target.TargetValueCalculation = _targetValueCalculation;
			target.UseIntraIntervalDeviation = _useIntraIntervalDeviation;
			target.UseTweakedValues = _useTweakedValues;

			target.UseMinimumStaffing = _useMinimumStaffing;
			target.UseMaximumStaffing = _useMaximumStaffing;
			target.UseMaximumSeats = _useMaximumSeats;
			target.DoNotBreakMaximumSeats = _doNotBreakMaximumSeats;

			target.RefreshScreenInterval = _refreshScreenInterval;
		}

		public void MapFrom(IAdvancedPreferences source)
		{
			_targetValueCalculation = source.TargetValueCalculation;
			_useIntraIntervalDeviation = source.UseIntraIntervalDeviation;
			_useTweakedValues = source.UseTweakedValues;

			_useMinimumStaffing = source.UseMinimumStaffing;
			_useMaximumStaffing = source.UseMaximumStaffing;
			_useMaximumSeats = source.UseMaximumSeats;
			_doNotBreakMaximumSeats = source.DoNotBreakMaximumSeats;

			_refreshScreenInterval = source.RefreshScreenInterval;
		}

		private void SetDefaultValues()
		{
			_targetValueCalculation = TargetValueOptions.StandardDeviation;
			_useMinimumStaffing = true;
			_useMaximumStaffing = true;
			_useMaximumSeats = true;
			_refreshScreenInterval = 10;
		}
	}
}
