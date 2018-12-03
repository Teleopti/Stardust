using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Extension methods for time zone info class
    /// </summary>
    public static class TimeZoneInfoExtensions
    {
		/// <summary>
		/// Converts a time to UTC and handles invalid times by avoiding them.
		/// </summary>
		/// <param name="timeZoneInfo">The source time zone.</param>
		/// <param name="dateTime">The datetime local to the time zone.</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static DateTime SafeConvertTimeToUtc(this TimeZoneInfo timeZoneInfo, DateTime dateTime)
        {
            var kindUnSpecifiedDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            while (timeZoneInfo.IsInvalidTime(kindUnSpecifiedDateTime))
            {
                kindUnSpecifiedDateTime = kindUnSpecifiedDateTime.AddMinutes(1);
            }
            return TimeZoneInfo.ConvertTimeToUtc(kindUnSpecifiedDateTime, timeZoneInfo);
        }
    }
}