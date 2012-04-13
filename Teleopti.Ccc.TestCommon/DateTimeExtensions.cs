using System;
using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Ccc.TestCommon
{
	public static class DateTimeExtensions
	{
		public static string ToShortDateString(this DateTime dateTime, CultureInfo culture) { return dateTime.ToString(culture.DateTimeFormat.ShortDatePattern); }

		public static string ToShortTimeString(this DateTime dateTime, CultureInfo culture) { return dateTime.ToString(culture.DateTimeFormat.ShortTimePattern); }

		public static IEnumerable<DateTime> TimeRange(this DateTime instance, DateTime toTime, TimeSpan step)
		{
			var time = instance;
			while (time <= toTime)
			{
				yield return time;
				time += step;
			}
		}

	}
}