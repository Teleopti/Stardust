using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Helper
{
	public static class DateExtensions
	{
		public static DateTime LimitMin(this DateTime dateTime)
		{
			if (DateHelper.MinSmallDateTime > dateTime)
				return DateHelper.MinSmallDateTime;

			return dateTime;
		}

		public static string ToShortDateString(this DateTime dateTime, CultureInfo culture)
		{
			return dateTime.ToString(culture.DateTimeFormat.ShortDatePattern);
		}

		public static string ToShortDateString(this DateTime dateTime, IUserCulture culture)
		{
			return dateTime.ToShortDateString(culture.GetCulture());
		}

		public static string ToShortTimeString(this DateTime dateTime, CultureInfo culture)
		{
			return dateTime.ToString(culture.DateTimeFormat.ShortTimePattern);
		}

		public static string ToShortTimeString(this DateTime dateTime, IUserCulture culture)
		{
			return dateTime.ToShortTimeString(culture.GetCulture());
		}
	}

	public static class CalendarExtensions
	{
		public static int GetMonth(this Calendar calendar, DateOnly date)
		{
			return calendar.GetMonth(date.Date);
		}
	}
}