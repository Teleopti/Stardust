using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class NightShiftOnMonday : ShiftForDate
	{
		public NightShiftOnMonday() : base(20) { }

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateHelper.GetFirstDateInWeek(DateOnly.Today, cultureInfo);
		}
	}
}