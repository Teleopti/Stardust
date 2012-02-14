using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class TimeZoneDateCache
    {
        private string _timeZoneInfoId;
        private DateTime _cachedLocalDateTime;
        private DateTime _cachedUtcDateTime;

        public DateTime GetLocalDateTime(DateTime dateTimeUtc, ICccTimeZoneInfo timeZoneInfo)
        {
            if (timeZoneInfo.Id!=_timeZoneInfoId || _cachedUtcDateTime!=dateTimeUtc)
            {
                _timeZoneInfoId = timeZoneInfo.Id;
                _cachedUtcDateTime = dateTimeUtc;
                _cachedLocalDateTime = timeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, timeZoneInfo).Date;
            }
            return _cachedLocalDateTime;
        }
    }
}
