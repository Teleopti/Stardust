using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public static string ToShortTimeString(this DateTime dateTime, CultureInfo culture)
		{
			return dateTime.ToString(culture.DateTimeFormat.ShortTimePattern);
		}

		public static string ToShortTimeString(this DateTime dateTime, IUserCulture culture)
		{
			return dateTime.ToShortTimeString(culture.GetCulture());
		}

        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
		
		public static DateTime Utc(this DateTime dateTime)
		{
			return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
		}
	}
}