using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class DayOffPreferenceOnWeekday : BasePreference
	{
		public int Weekday;
		public IDayOffTemplate DayOffTemplate = TestData.DayOffTemplate;

		public DayOffPreferenceOnWeekday(int weekday)
		{
			Weekday = weekday;
		}

		protected override PreferenceRestriction ApplyRestriction()
		{
			return new PreferenceRestriction { DayOffTemplate = DayOffTemplate };
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateHelper.GetFirstDateInWeek(DateTime.Now.Date, cultureInfo).AddDays(Weekday - 1);
		}
	}
}