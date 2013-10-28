using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class StandardPreference : BasePreference
	{
		public string Preference = TestData.DayOffTemplate.Description.Name;

		protected override PreferenceRestriction ApplyRestriction() { return new PreferenceRestriction { DayOffTemplate = TestData.DayOffTemplate }; }
		protected override DateTime ApplyDate(CultureInfo cultureInfo) { return DateHelper.GetFirstDateInWeek(DateOnlyForBehaviorTests.TestToday.Date, cultureInfo); }
	}
}