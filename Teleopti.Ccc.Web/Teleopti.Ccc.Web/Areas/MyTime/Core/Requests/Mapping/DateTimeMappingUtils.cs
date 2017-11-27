using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public static class DateTimeMappingUtils
	{
		public static String ConvertUtcToLocalDateTimeString(DateTime dateTime, TimeZoneInfo timeZoneInfo)
		{
			return convertToUserTimeZone (dateTime, timeZoneInfo).FormatDateTimeToIso8601String();
		}

		private static DateTime convertToUserTimeZone(DateTime dateTime, TimeZoneInfo timeZoneInfo)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo);
		}

		public static String FormatDateTimeToIso8601String(this DateTime dateTime)
		{
			var dateTimeUnspecified = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
			return dateTimeUnspecified.ToString("o");
		}
	}
}