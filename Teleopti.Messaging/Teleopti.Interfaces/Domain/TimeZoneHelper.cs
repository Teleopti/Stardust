using System;

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
        /// Converts to UTC, using the logged on users TimeZone.
        /// </summary>
        /// <param name="localDateTime">The local date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-23
        /// </remarks>
        public static DateTime ConvertToUtc(DateTime localDateTime)
        {
            TimeZoneInfo sourceTimeZone = CurrentSessionTimeZone;
            return sourceTimeZone.SafeConvertTimeToUtc(DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified));
        }

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
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-23
        /// </remarks>
        public static DateTime ConvertFromUtc(DateTime utcDateTime)
        {
            TimeZoneInfo targetTimeZone = CurrentSessionTimeZone;
            return ConvertFromUtc(utcDateTime, targetTimeZone);
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
        /// Creates a new DateTimePeriod from local date time.
        /// </summary>
        /// <param name="localDateTimeStart">The local date time start.</param>
        /// <param name="localDateTimeEnd">The local date time end.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-23
        /// </remarks>
        public static DateTimePeriod NewUtcDateTimePeriodFromLocalDateTime(DateTime localDateTimeStart, DateTime localDateTimeEnd)
        {
            return new DateTimePeriod(ConvertToUtc(localDateTimeStart), ConvertToUtc(localDateTimeEnd));
        }

        /// <summary>
        /// Gets the current sessions time zone.
        /// </summary>
        /// <value>The current sessions time zone.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-23
        /// </remarks>
        public static TimeZoneInfo CurrentSessionTimeZone
        {
            get { return StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone; }
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
            InParameter.NotNull("timeZone", timeZone);

            DateTime utcStartDateTime = timeZone.SafeConvertTimeToUtc(localStartDateTime);
            DateTime utcEndDateTime = timeZone.SafeConvertTimeToUtc(localEndDateTime);

            DateTimePeriod dateTimePeriod = new DateTimePeriod(utcStartDateTime, utcEndDateTime);
            return dateTimePeriod;
        }

	    public static DateTimePeriod NewUtcDateTimePeriodFromLocalDate(DateOnly localStartDate, DateOnly localEndDate, TimeZoneInfo timeZone)
		{
			return NewUtcDateTimePeriodFromLocalDateTime(localStartDate.Date, localEndDate.Date, timeZone);
	    }
    }
}