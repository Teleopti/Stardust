using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class ExistingPreferenceToday : BasePreference
	{
		public string Preference = TestData.Absence.Description.Name;

		protected override PreferenceRestriction ApplyRestriction() { return new PreferenceRestriction {Absence = TestData.Absence}; }
		protected override DateTime ApplyDate(CultureInfo cultureInfo) { return DateTime.Now.Date; }
	}
}