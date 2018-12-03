using System;
using System.Globalization;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Convert date and time between current sessions time zone and UTC.
	/// </summary>
	/// <remarks>
	/// Created by: robink
	/// Created date: 2007-10-23
	/// </remarks>
	public static class TimeZoneHelper
	{
		/// <summary>
		/// Converts to UTC.
		/// </summary>
		/// <param name="localDateTime">The local date time.</param>
		/// <param name="sourceTimeZone">The source time zone.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-10-23
		/// </remarks>
		public static DateTime ConvertToUtc(DateTime localDateTime, TimeZoneInfo sourceTimeZone)
		{
			return sourceTimeZone.SafeConvertTimeToUtc(DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified));
		}
		
		/// <summary>
		/// Converts from UTC.
		/// </summary>
		/// <param name="utcDateTime">The UTC date time.</param>
		/// <param name="sourceTimeZone">The source time zone.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-11-26
		/// </remarks>
		public static DateTime ConvertFromUtc(DateTime utcDateTime, TimeZoneInfo sourceTimeZone)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified),
											  sourceTimeZone);
		}
		
		/// <summary>
		/// Creates a new UTC format date time period from local date time.
		/// </summary>
		/// <param name="localStartDateTime">The local start date time.</param>
		/// <param name="localEndDateTime">The local end date time.</param>
		/// <param name="timeZone">The time zone.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-29
		/// </remarks>
		public static DateTimePeriod NewUtcDateTimePeriodFromLocalDateTime(DateTime localStartDateTime, DateTime localEndDateTime, TimeZoneInfo timeZone)
		{
			InParameter.NotNull(nameof(timeZone), timeZone);

			var utcStartDateTime = timeZone.SafeConvertTimeToUtc(localStartDateTime);
			var utcEndDateTime = timeZone.SafeConvertTimeToUtc(localEndDateTime);

			return new DateTimePeriod(utcStartDateTime, utcEndDateTime);
		}


		public static DateTimePeriod NewUtcDateTimePeriodFromLocalDate(DateOnly localStartDateTime, DateOnly localEndDateTime, TimeZoneInfo timeZone)
		{
			return NewUtcDateTimePeriodFromLocalDateTime(localStartDateTime.Date, localEndDateTime.Date, timeZone);
		}
	}
}