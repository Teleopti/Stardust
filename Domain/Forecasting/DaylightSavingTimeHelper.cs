using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public static class DaylightSavingTimeHelper
    {
        public static DateTime GetUtcEndTimeOfOneDay(DateTime localDateTime, TimeZoneInfo timeZone)
        {
            return GetUtcStartTimeOfOneDay(localDateTime.AddDays(1), timeZone);
        }

        public static DateTime GetUtcStartTimeOfOneDay(DateTime localDateTime, TimeZoneInfo timeZone)
        {
            InParameter.NotNull(nameof(timeZone), timeZone);
            var startTime = localDateTime;
            if (timeZone.IsInvalidTime(startTime))
            {
                do
                {
                    startTime = startTime.AddMinutes(1);
                } while (timeZone.IsInvalidTime(startTime));
            }

            return timeZone.SafeConvertTimeToUtc(startTime);
        }
    }
}
