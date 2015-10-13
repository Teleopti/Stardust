using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public static class DateTimeMappingUtils
	{
		public static String ConvertUtcToLocalDateTimeString(DateTime dateTime, TimeZoneInfo timeZoneInfo)
		{
			return formatDateTimeToIso8601String (convertToUserTimeZone (dateTime, timeZoneInfo));
		}

		private static DateTime convertToUserTimeZone(DateTime dateTime, TimeZoneInfo timeZoneInfo)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo);
		}

		private static String formatDateTimeToIso8601String(DateTime dateTime)
		{
			var dateTimeUnspecified = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
			return dateTimeUnspecified.ToString("o");
		}
	}
}