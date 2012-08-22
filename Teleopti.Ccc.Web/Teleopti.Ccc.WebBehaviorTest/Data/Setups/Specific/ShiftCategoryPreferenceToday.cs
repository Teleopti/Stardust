using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class ShiftCategoryPreferenceToday : BasePreference
	{
		public IShiftCategory ShiftCategory = TestData.ShiftCategory;

		protected override PreferenceRestriction ApplyRestriction()
		{
			return new PreferenceRestriction { ShiftCategory = ShiftCategory };
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateTime.Now.Date;
		}
	}
}