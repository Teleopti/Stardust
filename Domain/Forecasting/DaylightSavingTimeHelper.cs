using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public static class DaylightSavingTimeHelper
    {
        public static DateTime GetUtcEndTimeOfOneDay(DateTime localDateTime, ICccTimeZoneInfo timeZone)
        {
            return GetUtcStartTimeOfOneDay(localDateTime.AddDays(1), timeZone);
        }

        public static DateTime GetUtcStartTimeOfOneDay(DateTime localDateTime, ICccTimeZoneInfo timeZone)
        {
            InParameter.NotNull("timeZone", timeZone);
            var startTime = localDateTime;
            if (timeZone.IsInvalidTime(startTime))
            {
                do
                {
                    startTime = startTime.AddMinutes(1);
                } while (timeZone.IsInvalidTime(startTime));
            }

            return timeZone.ConvertTimeToUtc(startTime, timeZone);
        }
    }
}
