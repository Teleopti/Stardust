using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public static class DateTimeExtensions
	{
		public static string ToShortDateTimeString(this DateTime dateTime)
		{
			return $"{dateTime.ToShortDateString()} {dateTime.ToShortTimeString()}";
		}

		public static string ToShortDateTimeString(this DateTimePeriod period, TimeZoneInfo timeZone)
		{
			var startDateTime = period.StartDateTimeLocal(timeZone);
			var endDateTime = period.EndDateTimeLocal(timeZone);

			if (startDateTime.Date != endDateTime.Date)
				return string.Format("{0} {1} - {2} {3}", startDateTime.ToShortDateString(), startDateTime.ToShortTimeString(), endDateTime.ToShortDateString(), endDateTime.ToShortTimeString());
			return string.Format("{0} {1} - {2}", startDateTime.ToShortDateString(), startDateTime.ToShortTimeString(), endDateTime.ToShortTimeString());
		}

		public static string ToGregorianDateTimeString(this DateTime dateTime)
		{
			var calendar = new GregorianCalendar();
			var yearString = string.Format("{0:d4}", calendar.GetYear(dateTime));
			var monthString = string.Format("{0:d2}", calendar.GetMonth(dateTime));
			var dayString = string.Format("{0:d2}", calendar.GetDayOfMonth(dateTime));
			var timeString = string.Format("{0:HH:mm:ss}", dateTime);

			return string.Format(dateTime.Kind == DateTimeKind.Utc ? "{0}-{1}-{2}T{3}Z" : "{0}-{1}-{2}T{3}", yearString, monthString, dayString, timeString);
		}

		public static string ToShortDateOnlyString(this DateTimePeriod period, TimeZoneInfo timeZone)
        {
            var startDateTime = period.StartDateTimeLocal(timeZone);
            var endDateTime = period.EndDateTimeLocal(timeZone);

            if (startDateTime.Date != endDateTime.Date)
                return string.Format("{0} - {1} ", startDateTime.ToShortDateString(), endDateTime.ToShortDateString());
            return string.Format("{0}", startDateTime.ToShortDateString());
        }

		public static IEnumerable<TimeSpan> TimeRange(this TimeSpan instance, TimeSpan toTime, TimeSpan step)
		{
			var time = instance;
			while (time <= toTime)
			{
				yield return time;
				time += step;
			}
		}

		public static  IEnumerable<DateTime> TimeRange(this DateTime instance, DateTime toTime, TimeSpan step)
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