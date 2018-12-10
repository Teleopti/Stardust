using System;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class DaysOffPreferencesPersonalSettings : SettingValue
	{

		private bool _useKeepExistingDaysOff;
		private double _keepExistingDaysOffValue;

		private bool _useDaysOffPerWeek;
		private bool _useConsecutiveDaysOff;
		private bool _useConsecutiveWorkdays;
		private bool _useFullWeekendsOff;
		private bool _useWeekEndDaysOff;

		private int _daysOffPerWeekValueMin;
		private int _daysOffPerWeekValueMax;
		private int _consecutiveDaysOffValueMin;
		private int _consecutiveDaysOffValueMax;
		private int _consecutiveWorkdaysValueMin;
		private int _consecutiveWorkdaysValueMax;
		private int _fullWeekendsOffValueMin;
		private int _fullWeekendsOffValueMax;
		private int _weekEndDaysOffValueMin;
		private int _weekEndDaysOffValueMax;

		private bool _considerWeekBefore;
		private bool _considerWeekAfter;

		private bool _keepFreeWeekends;
		private bool _keepFreeWeekendDays;

		public DaysOffPreferencesPersonalSettings()
		{
			SetDefaultValues();
		}

		public void MapTo(IDaysOffPreferences target)
		{
			target.UseKeepExistingDaysOff = _useKeepExistingDaysOff;
			target.KeepExistingDaysOffValue = _keepExistingDaysOffValue;

			target.UseDaysOffPerWeek = _useDaysOffPerWeek;
			target.UseConsecutiveDaysOff = _useConsecutiveDaysOff;
			target.UseConsecutiveWorkdays = _useConsecutiveWorkdays;
			target.UseFullWeekendsOff = _useFullWeekendsOff;
			target.UseWeekEndDaysOff = _useWeekEndDaysOff;

			target.DaysOffPerWeekValue = new MinMax<int>(_daysOffPerWeekValueMin, _daysOffPerWeekValueMax);
			target.ConsecutiveDaysOffValue = new MinMax<int>(_consecutiveDaysOffValueMin, _consecutiveDaysOffValueMax);
			target.ConsecutiveWorkdaysValue = new MinMax<int>(_consecutiveWorkdaysValueMin, _consecutiveWorkdaysValueMax);
			target.FullWeekendsOffValue = new MinMax<int>(_fullWeekendsOffValueMin, _fullWeekendsOffValueMax);
			target.WeekEndDaysOffValue = new MinMax<int>(_weekEndDaysOffValueMin, _weekEndDaysOffValueMax);

			target.ConsiderWeekBefore = _considerWeekBefore;
			target.ConsiderWeekAfter = _considerWeekAfter;

			target.KeepFreeWeekends = _keepFreeWeekends;
			target.KeepFreeWeekendDays = _keepFreeWeekendDays;
		}

		public void MapFrom(IDaysOffPreferences source)
		{
			_useKeepExistingDaysOff = source.UseKeepExistingDaysOff;
			_keepExistingDaysOffValue = source.KeepExistingDaysOffValue;

			_useDaysOffPerWeek = source.UseDaysOffPerWeek;
			_useConsecutiveDaysOff = source.UseConsecutiveDaysOff;
			_useConsecutiveWorkdays = source.UseConsecutiveWorkdays;
			_useFullWeekendsOff = source.UseFullWeekendsOff;
			_useWeekEndDaysOff = source.UseWeekEndDaysOff;

			_daysOffPerWeekValueMin = source.DaysOffPerWeekValue.Minimum;
			_daysOffPerWeekValueMax = source.DaysOffPerWeekValue.Maximum;
			_consecutiveDaysOffValueMin = source.ConsecutiveDaysOffValue.Minimum;
			_consecutiveDaysOffValueMax = source.ConsecutiveDaysOffValue.Maximum;
			_consecutiveWorkdaysValueMin = source.ConsecutiveWorkdaysValue.Minimum;
			_consecutiveWorkdaysValueMax = source.ConsecutiveWorkdaysValue.Maximum;
			_fullWeekendsOffValueMin = source.FullWeekendsOffValue.Minimum;
			_fullWeekendsOffValueMax = source.FullWeekendsOffValue.Maximum;
			_weekEndDaysOffValueMin = source.WeekEndDaysOffValue.Minimum;
			_weekEndDaysOffValueMax = source.WeekEndDaysOffValue.Maximum;

			_considerWeekBefore = source.ConsiderWeekBefore;
			_considerWeekAfter = source.ConsiderWeekAfter;

			_keepFreeWeekends = source.KeepFreeWeekends;
			_keepFreeWeekendDays = source.KeepFreeWeekendDays;
		}

		private void SetDefaultValues()
		{

			_keepExistingDaysOffValue = 0d;

			_useDaysOffPerWeek = true;
			_useConsecutiveDaysOff = true;
			_useConsecutiveWorkdays = true;

			_daysOffPerWeekValueMin = 1;
			_daysOffPerWeekValueMax = 3;
			_consecutiveDaysOffValueMin = 1;
			_consecutiveDaysOffValueMax = 3;
			_consecutiveWorkdaysValueMin = 2;
			_consecutiveWorkdaysValueMax = 6;

			// ***** note: those 3 values are different in the old version
			//ConsiderWeekBefore = true;
			//KeepFreeWeekends = true;
			//KeepFreeWeekendDays = true;
		}
	}
}
