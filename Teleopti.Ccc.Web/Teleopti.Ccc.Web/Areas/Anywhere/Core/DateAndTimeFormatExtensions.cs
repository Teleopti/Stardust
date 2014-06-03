using System;
using System.Globalization;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public static class DateAndTimeFormatExtensions
	{
		public const string FixedDateFormat = "yyyy-MM-dd";
		public const string FixedDateTimeFormat = "yyyy-MM-dd HH:mm";
		public const string FixedDateTimeWithSecondsFormat = "yyyy-MM-dd HH:mm:ss";
		public const string FixedTimeFormat = "HH:mm";

		// see http://momentjs.com/docs/
		public static string FixedDateFormatForMoment = ToMomentFormat(FixedDateFormat);
		public static string FixedDateTimeFormatForMoment = ToMomentFormat(FixedDateTimeFormat);
		public static string FixedDateTimeWithSecondsFormatForMoment = ToMomentFormat(FixedDateTimeWithSecondsFormat);
		public static string FixedTimeFormatForMoment = ToMomentFormat(FixedTimeFormat);

		public static string DateFormat()
		{
			return CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
		}

		public static string DateTimeFormat()
		{
			return DateFormat() + " " + TimeFormat();
		}

		public static string TimeFormat()
		{
			return CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
		}

		public static string DateFormatForMoment()
		{
			return ToMomentFormat(DateFormat());
		}

		public static string DateTimeFormatForMoment()
		{
			return ToMomentFormat(DateTimeFormat());
		}

		public static string TimeFormatForMoment()
		{
			return ToMomentFormat(TimeFormat());
		}

		// http://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx to
		// http://momentjs.com/docs/ "formatting tokens" / LDML
		// https://docs.google.com/spreadsheet/ccc?key=0AtgZluze7WMJdDBOLUZfSFIzenIwOHNjaWZoeGFqbWc&hl=en_US#gid=0
		private static string ToMomentFormat(string format)
		{
			format = format.Replace("dddd", "dddd");
			format = format.Replace("ddd", "ddd");
			format = format.Replace("dd", "DD");
			format = format.Replace("d", "D");
			format = format.Replace("fff", "SSS");
			format = format.Replace("ff", "SS");
			format = format.Replace("f", "S");
			format = format.Replace("gg", "G");
			format = format.Replace("g", "G");
			format = format.Replace("hh", "hh");
			format = format.Replace("h", "h");
			format = format.Replace("HH", "HH");
			format = format.Replace("H", "H");
			format = format.Replace("K", "z");
			format = format.Replace("mm", "mm");
			format = format.Replace("m", "m");
			format = format.Replace("MMMM", "MMMM");
			format = format.Replace("MMM", "MMM");
			format = format.Replace("MM", "MM");
			format = format.Replace("M", "M");
			format = format.Replace("ss", "ss");
			format = format.Replace("s", "s");
			format = format.Replace("tt", "A");
			format = format.Replace("t", "A");
			format = format.Replace("yyyyy", "YYYY");
			format = format.Replace("yyyy", "YYYY");
			format = format.Replace("yyy", "YYYY");
			format = format.Replace("yy", "YY");
			format = format.Replace("y", "YY");
			format = format.Replace("zzz", "zzz");
			format = format.Replace("zz", "zz");
			format = format.Replace("z", "z");
			return format;
		}

		public static string ToFixedDateTimeFormat(this DateTime instance)
		{
			return instance.ToString(FixedDateTimeFormat);
		}

		public static string ToFixedDateFormat(this DateTime instance)
		{
			return instance.ToString(FixedDateFormat);
		}
	}
}