using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class ShiftCategoryPreference : BasePreference
	{
		public IShiftCategory ShiftCategory = TestData.ShiftCategory;

		protected override PreferenceRestriction ApplyRestriction() { return new PreferenceRestriction { ShiftCategory = ShiftCategory }; }
		protected override DateTime ApplyDate(CultureInfo cultureInfo) { return DateHelper.GetFirstDateInWeek(DateTime.Now.Date, cultureInfo); }
	}
}