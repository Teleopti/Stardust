using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveBackToLegalStateGui_44333)]
	public class SchedulingOptionsDayOffPlannerPersonalSettings : SettingValue
	{
		private bool _useDaysOffPerWeek = true;
		private int _minUseDaysOffPerWeek = 1;
		private int _maxUseDaysOffPerWeek = 3;

		private bool _useConsecutiveDaysOff = true;
		private int _minUseConsecutiveDaysOff = 1;
		private int _maxUseConsecutiveDaysOff = 3;

		private bool _useConsecutiveWorkdays = true;
		private int _minUseConsecutiveWorkdays = 1;
		private int _maxUseConsecutiveWorkdays = 6;

		private bool _useFullWeekendsOff;
		private int _minUseFullWeekendsOff;
		private int _maxUseFullWeekendsOff;

		private bool _useWeekEndDaysOff;
		private int _minUseWeekEndDaysOff;
		private int _maxUseWeekEndDaysOff;

		private bool _considerWeekBefore;
		private bool _considerWeekAfter;

		public void MapFrom(IDaysOffPreferences daysOffPreferences)
		{
			_useDaysOffPerWeek = daysOffPreferences.UseDaysOffPerWeek;
			_minUseDaysOffPerWeek = daysOffPreferences.DaysOffPerWeekValue.Minimum;
			_maxUseDaysOffPerWeek = daysOffPreferences.DaysOffPerWeekValue.Maximum;

			_useConsecutiveDaysOff = daysOffPreferences.UseConsecutiveDaysOff;
			_minUseConsecutiveDaysOff = daysOffPreferences.ConsecutiveDaysOffValue.Minimum;
			_maxUseConsecutiveDaysOff = daysOffPreferences.ConsecutiveDaysOffValue.Maximum;

			_useConsecutiveWorkdays = daysOffPreferences.UseConsecutiveWorkdays;
			_minUseConsecutiveWorkdays = daysOffPreferences.ConsecutiveWorkdaysValue.Minimum;
			_maxUseConsecutiveWorkdays = daysOffPreferences.ConsecutiveWorkdaysValue.Maximum;

			_useFullWeekendsOff = daysOffPreferences.UseFullWeekendsOff;
			_minUseFullWeekendsOff = daysOffPreferences.FullWeekendsOffValue.Minimum;
			_maxUseFullWeekendsOff = daysOffPreferences.FullWeekendsOffValue.Maximum;

			_useWeekEndDaysOff = daysOffPreferences.UseWeekEndDaysOff;
			_minUseWeekEndDaysOff = daysOffPreferences.WeekEndDaysOffValue.Minimum;
			_maxUseWeekEndDaysOff = daysOffPreferences.WeekEndDaysOffValue.Maximum;

			_considerWeekBefore = daysOffPreferences.ConsiderWeekBefore;
			_considerWeekAfter = daysOffPreferences.ConsiderWeekAfter;
		}

		public void MapTo(IDaysOffPreferences daysOffPreferences)
		{
			daysOffPreferences.UseDaysOffPerWeek = _useDaysOffPerWeek;
			daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(_minUseDaysOffPerWeek, _maxUseDaysOffPerWeek);

			daysOffPreferences.UseConsecutiveDaysOff = _useConsecutiveDaysOff;
			daysOffPreferences.ConsecutiveDaysOffValue = new MinMax<int>(_minUseConsecutiveDaysOff, _maxUseConsecutiveDaysOff);

			daysOffPreferences.UseConsecutiveWorkdays = _useConsecutiveWorkdays;
			daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(_minUseConsecutiveWorkdays, _maxUseConsecutiveWorkdays);

			daysOffPreferences.UseFullWeekendsOff = _useFullWeekendsOff;
			daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(_minUseFullWeekendsOff, _maxUseFullWeekendsOff);
			
			daysOffPreferences.UseWeekEndDaysOff = _useWeekEndDaysOff;
			daysOffPreferences.WeekEndDaysOffValue = new MinMax<int>(_minUseWeekEndDaysOff, _maxUseWeekEndDaysOff);
			
			daysOffPreferences.ConsiderWeekBefore = _considerWeekBefore;
			daysOffPreferences.ConsiderWeekAfter = _considerWeekAfter;
		}
	}
}
