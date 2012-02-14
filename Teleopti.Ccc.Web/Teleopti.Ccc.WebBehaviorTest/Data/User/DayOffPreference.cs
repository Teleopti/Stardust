using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class DayOffPreference : BasePreference
	{
		public IDayOffTemplate DayOffTemplate = TestData.DayOffTemplate;

		protected override PreferenceRestriction ApplyRestriction() { return new PreferenceRestriction {DayOffTemplate = DayOffTemplate}; }
		protected override DateTime ApplyDate(CultureInfo cultureInfo) { return DateHelper.GetFirstDateInWeek(DateTime.Now.Date, cultureInfo); }
	}
}