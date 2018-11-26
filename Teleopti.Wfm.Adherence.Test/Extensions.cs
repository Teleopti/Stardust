using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Wfm.Adherence.Test
{
	public static class Extensions
	{
//		public static IEnumerable<DateTime> TimeRange(this DateTime instance, DateTime toTime, TimeSpan step)
//		{
//			var time = instance;
//			while (time <= toTime)
//			{
//				yield return time;
//				time += step;
//			}
//		}
//
//		public static void Times(this int times, Action action)
//		{
//			Enumerable.Range(0, times).ForEach(i => action());
//		}
//
//		public static void Times(this int times, Action<int> action)
//		{
//			Enumerable.Range(0, times).ForEach(action);
//		}
//
//		public static TimeSpan Minutes(this string value)
//		{
//			return TimeSpan.FromMinutes(Convert.ToInt32(value));
//		}
//
//		public static TimeSpan Hours(this string value)
//		{
//			return TimeSpan.FromHours(Convert.ToInt32(value));
//		}
//
//		public static TimeSpan Seconds(this string value)
//		{
//			return TimeSpan.FromSeconds(Convert.ToInt32(value));
//		}
//
//		public static TimeSpan Milliseconds(this string value)
//		{
//			return TimeSpan.FromMilliseconds(Convert.ToInt32(value));
//		}
//
//		public static DateTimePeriod Period(this string periodString)
//		{
//			var parts = periodString.Split(" - ");
//			return new DateTimePeriod(parts[0].Utc(), parts[1].Utc());
//		}

		public static DateOnly Date(this string dateString)
		{
			return new DateOnly(dateString.Utc());
		}
		
		public static DateTime Utc(this string dateTimeString)
		{
			return DateTime.SpecifyKind(DateTime.Parse(dateTimeString, CultureInfo.GetCultureInfo("sv-SE")), DateTimeKind.Utc);
		}

//
//		public static DateTime In(this string dateTimeString, TimeZoneInfo timeZone)
//		{
//			return TimeZoneInfo.ConvertTimeFromUtc(dateTimeString.Utc(), timeZone);
//		}
//
//		public static DateTime In(this string dateTimeString, IUserTimeZone timeZone)
//		{
//			return TimeZoneInfo.ConvertTimeFromUtc(dateTimeString.Utc(), timeZone.TimeZone());
//		}
//
//		public static string AsShortTime(this DateTime dateTime, IUserCulture culture)
//		{
//			return dateTime.ToShortTimeString(culture);
//		}
//
//		public static string AsCatalanShortTime(this DateTime dateTime)
//		{
//			return dateTime.ToShortTimeString(CultureInfoFactory.CreateCatalanCulture());
//		}
	}
}
