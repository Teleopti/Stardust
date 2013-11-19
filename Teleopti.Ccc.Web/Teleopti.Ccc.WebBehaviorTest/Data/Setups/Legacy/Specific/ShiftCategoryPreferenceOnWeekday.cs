using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class ShiftCategoryPreferenceOnWeekday : BasePreference
	{
		public int Weekday;
		public IShiftCategory ShiftCategory = TestData.ShiftCategory;

		public ShiftCategoryPreferenceOnWeekday(int weekday)
		{
			Weekday = weekday;
		}

		protected override PreferenceRestriction ApplyRestriction()
		{
			return new PreferenceRestriction { ShiftCategory = ShiftCategory };
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateHelper.GetFirstDateInWeek(DateOnlyForBehaviorTests.TestToday.Date, cultureInfo).AddDays(Weekday - 1);
		}
	}
}