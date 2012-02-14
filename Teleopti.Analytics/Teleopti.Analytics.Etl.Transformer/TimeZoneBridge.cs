using System;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class TimeZoneBridge
    {
        private readonly DateTime _date;
        private readonly int _intervalId;
        private readonly DateTime _localDate;
        private readonly int _localIntervalId;
        private readonly string _timeZoneCode;

        private TimeZoneBridge()
        {
        }

        public TimeZoneBridge(DateTime date, TimeZoneInfo timeZoneInfo, int intervalsPerDay)
            : this()
        {
            ICccTimeZoneInfo cccTimeZoneInfo = new CccTimeZoneInfo(timeZoneInfo);
            var localDateTime = cccTimeZoneInfo.ConvertTimeFromUtc(date, cccTimeZoneInfo);

            _date = date.Date;
            _intervalId = new Interval(date, intervalsPerDay).Id;
            _timeZoneCode = timeZoneInfo.Id;
            _localDate = localDateTime.Date;
            _localIntervalId = new Interval(localDateTime, intervalsPerDay).Id;
        }

        public DateTime Date
        {
            get { return _date; }
        }

        public int IntervalId
        {
            get { return _intervalId; }
        }

        public string TimeZoneCode
        {
            get { return _timeZoneCode; }
        }

        public DateTime LocalDate
        {
            get { return _localDate; }
        }

        public int LocalIntervalId
        {
            get { return _localIntervalId; }
        }
    }
}
