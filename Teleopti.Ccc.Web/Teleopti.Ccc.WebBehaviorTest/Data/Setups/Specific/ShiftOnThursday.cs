using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class ShiftOnThursday : ShiftForDate
	{
		public ShiftOnThursday() : base(9) { }

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateHelper.GetFirstDateInWeek(DateOnly.Today, cultureInfo).AddDays(3);
		}
	}
}