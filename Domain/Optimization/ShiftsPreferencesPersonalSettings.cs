using System;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class ShiftsPreferencesPersonalSettings : SettingValue
	{
		
		private bool _keepShiftCategories;
		private bool _keepStartTimes;
        private bool _keepEndTimes;
		private bool _keepShifts;

		private double _keepShiftsValue;
		
        public ShiftsPreferencesPersonalSettings()
		{
			SetDefaultValues();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void MapTo(IShiftPreferences target)
		{
			

			target.KeepShiftCategories = _keepShiftCategories;
            target.KeepStartTimes = _keepStartTimes;
            target.KeepEndTimes = _keepEndTimes;
			target.KeepShifts = _keepShifts;

			target.KeepShiftsValue = _keepShiftsValue;
		
		}

        public void MapFrom(IShiftPreferences source)
		{
			

			_keepShiftCategories = source.KeepShiftCategories;
            _keepStartTimes = source.KeepStartTimes;
            _keepEndTimes = source.KeepEndTimes;
			_keepShifts = source.KeepShifts;

			_keepShiftsValue = source.KeepShiftsValue;
		}

		

		

		private void SetDefaultValues()
		{
			_keepShiftsValue = 0.8d;
		}

	}
}
