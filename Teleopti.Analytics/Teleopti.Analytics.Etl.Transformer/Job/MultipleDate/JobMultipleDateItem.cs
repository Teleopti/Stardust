using System;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.Job.MultipleDate
{
    public class JobMultipleDateItem : IJobMultipleDateItem
    {
        private readonly DateTime _startDateLocal;
        private readonly DateTime _endDateLocal;
        private readonly ICccTimeZoneInfo _cccTimeZoneInfo;

        public JobMultipleDateItem(DateTimeKind dateTimeKind, DateTime startDate, DateTime endDate, TimeZoneInfo timeZone)
        {
            _cccTimeZoneInfo = new CccTimeZoneInfo(timeZone);
            if (dateTimeKind == DateTimeKind.Utc)
            {
                //UTC incoming
                _startDateLocal = _cccTimeZoneInfo.ConvertTimeFromUtc(startDate, _cccTimeZoneInfo);
                _endDateLocal = _cccTimeZoneInfo.ConvertTimeFromUtc(endDate, _cccTimeZoneInfo);
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
            get { return _cccTimeZoneInfo.ConvertTimeToUtc(StartDateLocal); }
        }

        public DateTime EndDateUtc
        {
            get { return _cccTimeZoneInfo.ConvertTimeToUtc(EndDateLocal); }
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