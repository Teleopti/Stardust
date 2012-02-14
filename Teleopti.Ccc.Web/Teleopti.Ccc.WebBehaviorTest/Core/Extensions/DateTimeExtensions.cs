using System;
using System.Globalization;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Extensions
{
	public static class DateTimeExtensions
	{
		public static string ToShortDateString(this DateTime dateTime, CultureInfo culture)
		{
			return dateTime.ToString(culture.DateTimeFormat.ShortDatePattern);
		}

		public static string ToShortTimeString(this DateTime dateTime, CultureInfo culture)
		{
			return dateTime.ToString(culture.DateTimeFormat.ShortTimePattern);
		}

	}
}