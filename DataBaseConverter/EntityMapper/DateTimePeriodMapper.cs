using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps the specified DateTimePeriod
    /// </summary>
    public class DateTimePeriodMapper : Mapper<DateTimePeriod, global::Domain.TimePeriod>
    {
        private DateTime _date;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePeriodMapper"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public DateTimePeriodMapper(TimeZoneInfo timeZone, DateTime date)
                                : base(new MappedObjectPair(), timeZone)
        {
            _date = date;
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public override DateTimePeriod Map(global::Domain.TimePeriod oldEntity)
        {
            DateTime startTime = new DateTime(_date.Year, _date.Month, _date.Day).Add(oldEntity.StartTime);
            DateTime endTime = new DateTime(_date.Year, _date.Month, _date.Day).Add(oldEntity.EndTime);

            DateTime startDateTimeUtc = GetDateTimeAsUtc(startTime);
            DateTime endDateTimeUtc = GetDateTimeAsUtc(endTime);

            if (startDateTimeUtc > endDateTimeUtc) startDateTimeUtc = endDateTimeUtc;

            return new DateTimePeriod(startDateTimeUtc,
                                   endDateTimeUtc);
        }

        /// <summary>
        /// Gets the date time as UTC.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-13
        /// </remarks>
        private DateTime GetDateTimeAsUtc(DateTime dateTime)
        {
            if (TimeZone.IsInvalidTime(dateTime))
                return TimeZoneInfo.ConvertTimeToUtc(dateTime.AddHours(1), TimeZone);
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZone);
        }
    }
}