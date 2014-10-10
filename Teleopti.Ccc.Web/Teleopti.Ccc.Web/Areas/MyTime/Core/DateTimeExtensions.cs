using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public static class DateTimeExtensions
	{
		public static string ToShortDateTimeString(this DateTime dateTime)
		{
			return string.Format("{0} {1}", dateTime.ToShortDateString(), dateTime.ToShortTimeString());
		}

		public static string ToShortDateTimeString(this DateTimePeriod period, TimeZoneInfo timeZone)
		{
			var startDateTime = period.StartDateTimeLocal(timeZone);
			var endDateTime = period.EndDateTimeLocal(timeZone);

			if (startDateTime.Date != endDateTime.Date)
				return string.Format("{0} {1} - {2} {3}", startDateTime.ToShortDateString(), startDateTime.ToShortTimeString(), endDateTime.ToShortDateString(), endDateTime.ToShortTimeString());
			return string.Format("{0} {1} - {2}", startDateTime.ToShortDateString(), startDateTime.ToShortTimeString(), endDateTime.ToShortTimeString());
		}
        public static string ToShortDateOnlyString(this DateTimePeriod period, TimeZoneInfo timeZone)
        {
            var startDateTime = period.StartDateTimeLocal(timeZone);
            var endDateTime = period.EndDateTimeLocal(timeZone);

            if (startDateTime.Date != endDateTime.Date)
                return string.Format("{0} - {1} ", startDateTime.ToShortDateString(), endDateTime.ToShortDateString());
            return string.Format("{0}", startDateTime.ToShortDateString());
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