using System;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class DaysOffPreferencesPersonalSettings : SettingValue, IPersonalSettings<IDaysOffPreferences>
	{

		private bool UseKeepExistingDaysOff { get; set; }
		private double KeepExistingDaysOffValue { get; set; }

		private bool UseDaysOffPerWeek { get; set; }
		private bool UseConsecutiveDaysOff { get; set; }
		private bool UseConsecutiveWorkdays { get; set; }
		private bool UseFullWeekendsOff { get; set; }
		private bool UseWeekEndDaysOff { get; set; }

		private int DaysOffPerWeekValueMin { get; set; }
		private int DaysOffPerWeekValueMax { get; set; }
		private int ConsecutiveDaysOffValueMin { get; set; }
		private int ConsecutiveDaysOffValueMax { get; set; }
		private int ConsecutiveWorkdaysValueMin { get; set; }
		private int ConsecutiveWorkdaysValueMax { get; set; }
		private int FullWeekendsOffValueMin { get; set; }
		private int FullWeekendsOffValueMax { get; set; }
		private int WeekEndDaysOffValueMin { get; set; }
		private int WeekEndDaysOffValueMax { get; set; }

		private bool ConsiderWeekBefore { get; set; }
		private bool ConsiderWeekAfter { get; set; }

		private bool KeepFreeWeekends { get; set; }
		private bool KeepFreeWeekendDays { get; set; }

		public DaysOffPreferencesPersonalSettings()
		{
			MapFrom(new DaysOffPreferences());
		}

		public void MapTo(IDaysOffPreferences target)
		{
			target.UseKeepExistingDaysOff = UseKeepExistingDaysOff;
			target.KeepExistingDaysOffValue = KeepExistingDaysOffValue;

			target.UseDaysOffPerWeek = UseDaysOffPerWeek;
			target.UseConsecutiveDaysOff = UseConsecutiveDaysOff;
			target.UseConsecutiveWorkdays = UseConsecutiveWorkdays;
			target.UseFullWeekendsOff = UseFullWeekendsOff;
			target.UseWeekEndDaysOff = UseWeekEndDaysOff;

			target.DaysOffPerWeekValue = new MinMax<int>(DaysOffPerWeekValueMin, DaysOffPerWeekValueMax);
			target.ConsecutiveDaysOffValue = new MinMax<int>(ConsecutiveDaysOffValueMin, ConsecutiveDaysOffValueMax);
			target.ConsecutiveWorkdaysValue = new MinMax<int>(ConsecutiveWorkdaysValueMin, ConsecutiveWorkdaysValueMax);
			target.FullWeekendsOffValue = new MinMax<int>(FullWeekendsOffValueMin, FullWeekendsOffValueMax);
			target.WeekEndDaysOffValue = new MinMax<int>(WeekEndDaysOffValueMin, WeekEndDaysOffValueMax);

			target.ConsiderWeekBefore = ConsiderWeekBefore;
			target.ConsiderWeekAfter = ConsiderWeekAfter;

			target.KeepFreeWeekends = KeepFreeWeekends;
			target.KeepFreeWeekendDays = KeepFreeWeekendDays;
		}

		public void MapFrom(IDaysOffPreferences source)
		{
			UseKeepExistingDaysOff = source.UseKeepExistingDaysOff;
			KeepExistingDaysOffValue = source.KeepExistingDaysOffValue;

			UseDaysOffPerWeek = source.UseDaysOffPerWeek;
			UseConsecutiveDaysOff = source.UseConsecutiveDaysOff;
			UseConsecutiveWorkdays = source.UseConsecutiveWorkdays;
			UseFullWeekendsOff = source.UseFullWeekendsOff;
			UseWeekEndDaysOff = source.UseWeekEndDaysOff;

			DaysOffPerWeekValueMin = source.DaysOffPerWeekValue.Minimum;
			DaysOffPerWeekValueMax = source.DaysOffPerWeekValue.Maximum;
			ConsecutiveDaysOffValueMin = source.ConsecutiveDaysOffValue.Minimum;
			ConsecutiveDaysOffValueMax = source.ConsecutiveDaysOffValue.Maximum;
			ConsecutiveWorkdaysValueMin = source.ConsecutiveWorkdaysValue.Minimum;
			ConsecutiveWorkdaysValueMax = source.ConsecutiveWorkdaysValue.Maximum;
			FullWeekendsOffValueMin = source.FullWeekendsOffValue.Minimum;
			FullWeekendsOffValueMax = source.FullWeekendsOffValue.Maximum;
			WeekEndDaysOffValueMin = source.WeekEndDaysOffValue.Minimum;
			WeekEndDaysOffValueMax = source.WeekEndDaysOffValue.Maximum;

			ConsiderWeekBefore = source.ConsiderWeekBefore;
			ConsiderWeekAfter = source.ConsiderWeekAfter;

			KeepFreeWeekends = source.KeepFreeWeekends;
			KeepFreeWeekendDays = source.KeepFreeWeekendDays;
		}
	}
}
