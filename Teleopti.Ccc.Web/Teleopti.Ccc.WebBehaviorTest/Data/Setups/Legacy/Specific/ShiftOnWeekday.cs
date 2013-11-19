using System;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class ShiftOnWeekday : ShiftForDate
	{
		public int Weekday { get; set; }

		public ShiftOnWeekday(TimeSpan startTime, TimeSpan endTime, int weekday) : base(startTime, endTime, false)
		{
			Weekday = weekday;
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateHelper.GetFirstDateInWeek(DateOnlyForBehaviorTests.TestToday.Date, cultureInfo).AddDays(Weekday - 1);
		}
	}
}