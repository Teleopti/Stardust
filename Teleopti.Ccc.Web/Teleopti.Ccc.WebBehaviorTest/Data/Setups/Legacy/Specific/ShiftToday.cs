using System;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class ShiftToday : ShiftForDate
	{
		public ShiftToday() : base(9) { }

		public ShiftToday(int startHour) : base(startHour) { }

		public ShiftToday(TimeSpan startTime, TimeSpan endTime) : base(startTime, endTime) { }

		public ShiftToday(TimeSpan startTime, TimeSpan endTime, bool withLunch) : base(startTime, endTime, withLunch) { }

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateOnlyForBehaviorTests.TestToday.Date;
		}
	}
}