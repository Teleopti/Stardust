using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class AbsencePreferenceOnWeekday : BasePreference
	{
		public int Weekday;
		public IAbsence Absence = TestData.Absence;

		public AbsencePreferenceOnWeekday(int weekday)
		{
			Weekday = weekday;
		}

		protected override PreferenceRestriction ApplyRestriction()
		{
			return new PreferenceRestriction { Absence = Absence };
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateHelper.GetFirstDateInWeek(DateTime.Now.Date, cultureInfo).AddDays(Weekday - 1);
		}
	}
}