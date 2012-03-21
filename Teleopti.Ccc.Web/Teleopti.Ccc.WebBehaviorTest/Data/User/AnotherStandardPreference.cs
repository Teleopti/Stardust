using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class AnotherStandardPreference : BasePreference
	{
		public string Preference = TestData.Absence.Description.Name;

		protected override PreferenceRestriction ApplyRestriction() { return new PreferenceRestriction { Absence = TestData.Absence }; }
		protected override DateTime ApplyDate(CultureInfo cultureInfo) { return DateHelper.GetFirstDateInWeek(DateTime.Now.Date, cultureInfo).AddDays(1); }
	}
}