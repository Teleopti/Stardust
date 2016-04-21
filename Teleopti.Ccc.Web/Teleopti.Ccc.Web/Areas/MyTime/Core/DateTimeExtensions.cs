using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;
using static System.String;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public static class DateTimeExtensions
	{
		public static string ToShortDateTimeString(this DateTime dateTime)
		{
			return Format("{0} {1}", dateTime.ToShortDateString(), dateTime.ToShortTimeString());
		}

		public static string ToShortDateTimeString(this DateTimePeriod period, TimeZoneInfo timeZone)
		{
			var startDateTime = period.StartDateTimeLocal(timeZone);
			var endDateTime = period.EndDateTimeLocal(timeZone);

			if (startDateTime.Date != endDateTime.Date)
				return Format("{0} {1} - {2} {3}", startDateTime.ToShortDateString(), startDateTime.ToShortTimeString(), endDateTime.ToShortDateString(), endDateTime.ToShortTimeString());
			return Format("{0} {1} - {2}", startDateTime.ToShortDateString(), startDateTime.ToShortTimeString(), endDateTime.ToShortTimeString());
		}

		public static string ToGregorianDateTimeString(this DateTime dateTime)
		{
			var calendar = new GregorianCalendar();
			var yearString = Format("{0:d4}", calendar.GetYear(dateTime));
			var monthString = Format("{0:d2}", calendar.GetMonth(dateTime));
			var dayString = Format("{0:d2}", calendar.GetDayOfMonth(dateTime));
			var timeString = Format("{0:HH:mm:ss}", dateTime);

			return Format(dateTime.Kind == DateTimeKind.Utc ? "{0}-{1}-{2}T{3}Z" : "{0}-{1}-{2}T{3}", yearString, monthString, dayString, timeString);
		}

		public static string ToShortDateOnlyString(this DateTimePeriod period, TimeZoneInfo timeZone)
        {
            var startDateTime = period.StartDateTimeLocal(timeZone);
            var endDateTime = period.EndDateTimeLocal(timeZone);

            if (startDateTime.Date != endDateTime.Date)
                return Format("{0} - {1} ", startDateTime.ToShortDateString(), endDateTime.ToShortDateString());
            return Format("{0}", startDateTime.ToShortDateString());
        }

		public static IEnumerable<DateTime> DateRange(this DateTime instance, int days)
		{
			return from i in Enumerable.Range(0, days) select instance.AddDays(i);
		}

		public static IEnumerable<DateOnly> DateRange(this DateOnly instance, int days)
		{
			return from i in Enumerable.Range(0, days) select instance.AddDays(i);
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