using System;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.Job.MultipleDate
{
    public class JobMultipleDateItem : IJobMultipleDateItem
    {
        private readonly DateTime _startDateLocal;
        private readonly DateTime _endDateLocal;
        private readonly TimeZoneInfo _timeZoneInfo;

        public JobMultipleDateItem(DateTimeKind dateTimeKind, DateTime startDate, DateTime endDate, TimeZoneInfo timeZone)
        {
            _timeZoneInfo = timeZone;
            if (dateTimeKind == DateTimeKind.Utc)
            {
                //UTC incoming
                _startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDate, _timeZoneInfo);
                _endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDate, _timeZoneInfo);
            }
            else
            {
                // Local incoming
                _startDateLocal = startDate;
                _endDateLocal = endDate;
            }
        }

        public DateTime StartDateLocal
        {
            get
            {
                return _startDateLocal;
            }
        }
        public DateTime StartDateUtc
        {
            get { return _timeZoneInfo.SafeConvertTimeToUtc(StartDateLocal); }
        }

        public DateTime EndDateUtc
        {
            get { return _timeZoneInfo.SafeConvertTimeToUtc(EndDateLocal); }
        }

        public DateTime StartDateUtcFloor
        {
            get { return StartDateUtc.Date; }
        }

        public DateTime EndDateUtcCeiling
        {
            get { return EndDateUtc.Date.AddDays(1).AddMilliseconds(-1); }
        }

        public DateTime EndDateLocal
        {
            get
            {
                return _endDateLocal;
            }
        }
    }
}