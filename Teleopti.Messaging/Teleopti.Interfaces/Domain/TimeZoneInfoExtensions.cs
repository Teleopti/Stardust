using System;

namespace Teleopti.Interfaces.Domain
{
    public static class TimeZoneInfoExtensions
    {
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