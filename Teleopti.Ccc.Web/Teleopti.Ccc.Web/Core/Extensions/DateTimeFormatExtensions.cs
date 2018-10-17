using System;
using System.Globalization;

namespace Teleopti.Ccc.Web.Core.Extensions
{
	public static class DateTimeFormatExtensions
	{
		public const string FixedDateFormat = "yyyy-MM-dd";
		public const string FixedDateTimeFormat = "yyyy-MM-dd HH:mm";

		public static string LocalizedDateFormat => CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
		public static string LocalizedTimeFormat => CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
		public static DayOfWeek FirstDayOfWeek => CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

		public static string ToFixedDateTimeFormat(this DateTime instance)
		{
			return instance.ToString(FixedDateTimeFormat);
		}

		public static string ToFixedDateFormat(this DateTime instance)
		{
			return instance.ToString(FixedDateFormat);
		}

		public static string ToLocalizedDateTimeFormatWithTSpliting(this DateTime instance)
		{
			return instance.ToString($"{LocalizedDateFormat}T{LocalizedTimeFormat}");
		}

		public static string ToLocalizedTimeFormat(this DateTime instance)
		{
			return instance.ToString(LocalizedTimeFormat);
		}
	}
}