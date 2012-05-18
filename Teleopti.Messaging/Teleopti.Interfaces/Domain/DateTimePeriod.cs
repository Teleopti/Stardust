using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// DateTimePeriod structure that holds information on an action with a certain start and end time in UTC. 
    /// </summary>
    [Serializable]
	public struct DateTimePeriod
    {
        private MinMax<DateTime> period;
        private const string DATETIME_SEPARATOR = " - ";

        #region Properties


        /// <summary>
        /// Gets the start date time in UTC.
        /// </summary>
        /// <value>The start date time.</value>
        public DateTime StartDateTime
        {
            get { return period.Minimum; }
        }

        /// <summary>
        /// Gets the end date time in UTC.
        /// </summary>
        /// <value>The end date time.</value>
        public DateTime EndDateTime
        {
            get { return period.Maximum; }
        }

        /// <summary>
        /// Gets the local start date time.
        /// </summary>
        /// <value>The local start date time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-23
        /// </remarks>
        public DateTime LocalStartDateTime
        {
            get { return TimeZoneHelper.ConvertFromUtc(StartDateTime); }
        }

        /// <summary>
        /// Gets the local end date time.
        /// </summary>
        /// <value>The local end date time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-23
        /// </remarks>
        public DateTime LocalEndDateTime
        {
            get
            {
                return TimeZoneHelper.ConvertFromUtc(EndDateTime);
            }
        }

        /// <summary>
        /// Starts the date time local.
        /// </summary>
        /// <param name="timeZoneInfo">The time zone info.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-03-19
        /// </remarks>
        public DateTime StartDateTimeLocal(ICccTimeZoneInfo timeZoneInfo)
        {
            return TimeZoneHelper.ConvertFromUtc(StartDateTime, timeZoneInfo);
        }

        /// <summary>
        /// Ends the date time local.
        /// </summary>
        /// <param name="timeZoneInfo">The time zone info.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-03-19
        /// </remarks>
        public DateTime EndDateTimeLocal(ICccTimeZoneInfo timeZoneInfo)
        {
            return TimeZoneHelper.ConvertFromUtc(EndDateTime, timeZoneInfo);
        }

        #endregion

        #region Constructors

        private DateTimePeriod(DateTime startDateTime, DateTime endDateTime, bool validate)
        {
            if(validate)
                validateDateTime(startDateTime, endDateTime);

            period = new MinMax<DateTime>(startDateTime, endDateTime);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePeriod"/> class.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        public DateTimePeriod(DateTime startDateTime, DateTime endDateTime)
        {
            validateDateTime(startDateTime, endDateTime);
            period = new MinMax<DateTime>(startDateTime, endDateTime);
        }

        /// <summary>
        /// Validates the date and time.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-18
        /// </remarks>
        private static void validateDateTime(DateTime startDateTime, DateTime endDateTime)
        {
            InParameter.VerifyDateIsUtc("startDateTime", startDateTime);
            InParameter.VerifyDateIsUtc("endDateTime", endDateTime);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePeriod"/> class.
        /// </summary>
        /// <param name="startYear">The start year.</param>
        /// <param name="startMonth">The start month.</param>
        /// <param name="startDay">The start day.</param>
        /// <param name="endYear">The end year.</param>
        /// <param name="endMonth">The end month.</param>
        /// <param name="endDay">The end day.</param>
        public DateTimePeriod(int startYear,
                              int startMonth,
                              int startDay,
                              int endYear,
                              int endMonth,
                              int endDay)
        {
            DateTime startDateTimeTemp = new DateTime(startYear, startMonth, startDay, 0, 0, 0, DateTimeKind.Utc);
            DateTime endDateTimeTemp = new DateTime(endYear, endMonth, endDay, 0, 0, 0, DateTimeKind.Utc);

            validateDateTime(startDateTimeTemp, endDateTimeTemp);
            period = new MinMax<DateTime>(startDateTimeTemp, endDateTimeTemp);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a TimeSpan representing the Elapsed time in the TimePeriod.
        /// </summary>
        /// <returns></returns>
        public TimeSpan ElapsedTime()
        {
            return EndDateTime.Subtract(StartDateTime);
        }

        /// <summary>
        /// Moves the period the spacific amount of time.
        /// </summary>
        /// <param name="timeSpan">A time span.</param>
        /// <returns></returns>
        public DateTimePeriod MovePeriod(TimeSpan timeSpan)
        {
            if (timeSpan.Ticks > 0)
                return ChangeEndTime(timeSpan).ChangeStartTime(timeSpan);
            return ChangeStartTime(timeSpan).ChangeEndTime(timeSpan);
        }

        /// <summary>
        /// Returns if the param DateTimePeriod is contained entirely
        /// </summary>
        /// <param name="containsPeriod">The period.</param>
        /// <returns>
        /// 	<c>true</c> if the specified period is contained; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(DateTimePeriod containsPeriod)
        {
            return (ContainsPart(containsPeriod.StartDateTime) && ContainsPart(containsPeriod.EndDateTime));
        }

        /// <summary>
        /// Determines whether the specified date is contained within this period.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified date]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(DateTime date)
        {
            //InParameter.VerifyDateIsUtc("date", date);
            return (period.Minimum <= date && period.Maximum > date);
        }

        /// <summary>
        /// Returns the common period shared by this and param DateTimePeriod.
        /// </summary>
        /// <param name="periodToValidate">The period.</param>
        /// <returns>
        /// </returns>
        public DateTimePeriod SharedPeriod(DateTimePeriod periodToValidate)
        {

            if (!ContainsPart(periodToValidate))
                throw new ArgumentOutOfRangeException("periodToValidate", "This operation can not be performed on the supplied DateTimePeriod.");

            DateTime start, end;

            if (periodToValidate.StartDateTime >= StartDateTime)
                start = periodToValidate.StartDateTime;
            else
                start = StartDateTime;

            if (periodToValidate.EndDateTime <= EndDateTime)
                end = periodToValidate.EndDateTime;
            else
                end = EndDateTime;

            return new DateTimePeriod(start, end, false);
        }

        /// <summary>
        /// Returns if any part of the param DateTimePeriod is contained. 
        /// </summary>
        /// <param name="containsPeriod">The period.</param>
        /// <returns>
        /// 	<c>true</c> if the specified period is contained; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsPart(DateTimePeriod containsPeriod)
        {
            if (ContainsPart(containsPeriod.StartDateTime) || ContainsPart(containsPeriod.EndDateTime))
                return true;

            if (containsPeriod.Contains(this))
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether the specified period contains part.
        /// </summary>
        /// <param name="containsPeriod">The period.</param>
        /// <param name="includeStartAndEnd">if set to <c>true</c> [include start and end].</param>
        /// <returns>
        /// 	<c>true</c> if the specified period contains part; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-12
        /// </remarks>
        public bool ContainsPart(DateTimePeriod containsPeriod, bool includeStartAndEnd)
        {
            if (!includeStartAndEnd)
            {
                if (containsPeriod.StartDateTime == containsPeriod.EndDateTime ||
                    containsPeriod.ElapsedTime().TotalMinutes <= 1d)
                {
                    return Contains(containsPeriod);
                }

                containsPeriod = containsPeriod.ChangeStartTime(new TimeSpan(1L));
                containsPeriod = containsPeriod.ChangeEndTime(new TimeSpan(-1L));
            }

            return ContainsPart(containsPeriod);
        }

        /// <summary>
        /// Returns if any part of the param DateTime is contained
        /// </summary>
        /// <param name="theDateTime"></param>
        /// <returns><c>true</c> if the specified period is contained; otherwise, <c>false</c>.</returns>
        public bool ContainsPart(DateTime theDateTime)
        {
            InParameter.VerifyDateIsUtc("theDateTime", theDateTime);

            return (theDateTime <= EndDateTime && theDateTime >= StartDateTime);
        }

        /// <summary>
        /// Changes the end time according to supplied timespan
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public DateTimePeriod ChangeEndTime(TimeSpan timeSpan)
        {
            return new DateTimePeriod(StartDateTime, EndDateTime.Add(timeSpan), false);
        }

        /// <summary>
        /// Changes the start time according to supplied timespan
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public DateTimePeriod ChangeStartTime(TimeSpan timeSpan)
        {
            return new DateTimePeriod(StartDateTime.Add(timeSpan), EndDateTime, false);
        }

        /// <summary>
        /// Gets every UTC date in this DateTimePeriod as a collection.
        /// </summary>
        /// <value>The date collection.</value>
        public IList<DateTime> DateCollection()
        {
            IList<DateTime> returnList = new List<DateTime>();

            DateTime startDay = StartDateTime.Date;
            DateTime endDay = EndDateTime.Date;

            while (startDay <= endDay)
            {
                returnList.Add(startDay);
                startDay = startDay.Date.AddDays(1);
            }
            return returnList;
        }

        /// <summary>
        /// Returns an IList of DateTimePeriods corresponding to each affected hour.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-29
        /// </remarks>
        public IList<DateTimePeriod> AffectedHourCollection()
        {
            DateTime endDateTime = EndDateTime.Date.AddHours(EndDateTime.Hour).AddHours(1);
            DateTime startDateTime = StartDateTime.Date.AddHours(StartDateTime.Hour);

            IList<DateTimePeriod> retList = new List<DateTimePeriod>();
            for (; startDateTime < endDateTime; startDateTime=startDateTime.AddHours(1))
            {
                retList.Add(new DateTimePeriod(startDateTime,startDateTime.AddHours(1)));
            }

            return retList;

        }

        /// <summary>
        /// Whole days collection. Uses the start time of period to return whole days (24 hours) after that. Only last period will differ from 24 hours length.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-21
        /// </remarks>
        public IList<DateTimePeriod> WholeDayCollection()
        {
            IList<DateTimePeriod> collectionToReturn = new List<DateTimePeriod>();
            DateTime currentDateTime = LocalStartDateTime;
            DateTime endDateTime = LocalEndDateTime;
            while (currentDateTime < endDateTime)
            {
                DateTime currentEndDateTime = currentDateTime.AddDays(1);
                if (currentEndDateTime > endDateTime) currentEndDateTime = endDateTime;
                if (endDateTime == currentDateTime) break;

                collectionToReturn.Add(TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(currentDateTime,
                                                                                            currentEndDateTime));
                currentDateTime = currentEndDateTime;
            }
            return collectionToReturn;
        }

        /// <summary>
        /// The local days affected.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-21
        /// </remarks>
        public IList<DateTime> LocalDaysAffected()
        {
            IList<DateTimePeriod> wholeDayCollection = WholeDayCollection();
            IList<DateTime> collectionToReturn = new List<DateTime>();
            foreach (DateTimePeriod wholeDay in wholeDayCollection)
            {
                if (!collectionToReturn.Contains(wholeDay.LocalStartDateTime.Date))
                    collectionToReturn.Add(wholeDay.LocalStartDateTime.Date);
                if (!collectionToReturn.Contains(wholeDay.LocalEndDateTime.AddTicks(-1L).Date))
                    collectionToReturn.Add(wholeDay.LocalEndDateTime.AddTicks(-1L).Date);
            }
            return collectionToReturn;
        }

        /// <summary>
        /// The days affected in UTC.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-21
        /// </remarks>
        public IList<DateTime> UtcDaysAffected()
        {
            IList<DateTimePeriod> wholeDayCollection = WholeDayCollection();
            IList<DateTime> collectionToReturn = new List<DateTime>();
            foreach (DateTimePeriod wholeDay in wholeDayCollection)
            {
                if (!collectionToReturn.Contains(wholeDay.StartDateTime.Date))
                    collectionToReturn.Add(wholeDay.StartDateTime.Date);
                if (!collectionToReturn.Contains(wholeDay.EndDateTime.Date))
                    collectionToReturn.Add(wholeDay.EndDateTime.Date);
            }
            return collectionToReturn;
        }

        /// <summary>
        /// Gets a list of TimeSpans, one for each interval in the DateTimePeriod.
        /// Will take summer time and winter time changes into consideration.
        /// When moving from winter to summer time, the TimeSpan 02:00- will not exists
        /// and when moving from summer to winter time, the TimeSpan 02:00- will occur twice.
        /// </summary>
        /// <param name="resolution">The resolution.</param>
        /// <param name="timeZoneInfo">The time zone info.</param>
        /// <returns></returns>
        /// /// 
        /// <remarks>
        /// Created by: cs
        /// Created date: 2000-03-26
        /// </remarks>
        public IList<IntervalDefinition> IntervalsFromHourCollection(int resolution, ICccTimeZoneInfo timeZoneInfo)
        {
            var elapsedTime = ElapsedTime();
            if (elapsedTime == TimeSpan.Zero) return new List<IntervalDefinition>();

            if (resolution <= 0)
                throw new ArgumentException("resolution to low");

            if (StartDateTime.Add(TimeSpan.FromMinutes(resolution)) > EndDateTime)
                throw new ArgumentException("resolution to high");

            // Removed this check due to bug 9873
            //if ((int)elapsedTime.TotalMinutes % resolution != 0)
            //    throw new ArgumentException("period modulus resolution has to be 0");

            IList<IntervalDefinition> intervals = new List<IntervalDefinition>();
            var resolutionAsTimeSpan = TimeSpan.FromMinutes(resolution);
            var localStartDate = StartDateTimeLocal(timeZoneInfo).Date;
            var currentTime = StartDateTime;
            do
            {
                var timeRelativeToStartOfDay =
                    timeZoneInfo.ConvertTimeFromUtc(currentTime, timeZoneInfo).Subtract(localStartDate);
                intervals.Add(new IntervalDefinition(currentTime,timeRelativeToStartOfDay));
                currentTime = currentTime.Add(resolutionAsTimeSpan);
            } while (currentTime<EndDateTime);

            return intervals;
        }


        #endregion

        #region Equals stuff

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(DateTimePeriod other)
        {
            return other.StartDateTime == StartDateTime &&
                   other.EndDateTime == EndDateTime;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if obj and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is DateTimePeriod))
            {
                return false;
            }
            return Equals((DateTimePeriod) obj);
        }

        /// <summary>
        /// Operator ==.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator ==(DateTimePeriod per1, DateTimePeriod per2)
        {
            return per1.Equals(per2);
        }

        /// <summary>
        /// Operator !=.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator !=(DateTimePeriod per1, DateTimePeriod per2)
        {
            return !per1.Equals(per2);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return (string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}",GetType().FullName,
                    StartDateTime.Ticks, EndDateTime.Ticks)).GetHashCode();
        }

        /// <summary>
        /// Get Object Data
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("StartDateTime", period.Minimum);
            info.AddValue("EndDateTime", period.Maximum);
        }

        #endregion

        #region IComparable<DateTimePeriod> Members

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-06
        /// </remarks>
        public int CompareTo(DateTimePeriod other)
        {
            if (StartDateTime.Equals(other.StartDateTime))
                return EndDateTime.CompareTo(other.EndDateTime);

            return StartDateTime.CompareTo(other.StartDateTime);
        }

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-06
        /// </remarks>
        public static bool operator <(DateTimePeriod per1, DateTimePeriod per2)
        {
            return (per1.ElapsedTime() < per2.ElapsedTime());
        }

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-06
        /// </remarks>
        public static bool operator >(DateTimePeriod per1, DateTimePeriod per2)
        {
            return (per1.ElapsedTime() > per2.ElapsedTime());
        }

        #endregion

        /// <summary>
        /// Returns if intersects with the specified date time period.
        /// </summary>
        /// <param name="intersectPeriod">The date time period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 6.12.2007
        /// Seems to have the same behavor as ContainsPart(period, false)
        /// Yes, but this is much faster code, ContansPart(period) should be replaced by this.
        /// </remarks>
        public bool Intersect(DateTimePeriod intersectPeriod)
        {
            return !((StartDateTime >= intersectPeriod.EndDateTime) || (EndDateTime <= intersectPeriod.StartDateTime));
        }

        /// <summary>
        /// Gets the time period representing the intersection of the two periods.
        /// If the periods does not intersect, or the end time equals with the other periods start time then null is returned
        /// </summary>
        /// <param name="intersectPeriod">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 4.12.2007
        /// The method is very similar to the SharedPeriod method. The differences are the following:
        /// - If the two period does not have common part, the Intersection method will return a null value, while SharedPeriod will
        /// throw an exception.
        /// - If one period end time equals the other period start time, then the Intersect will return a null value, while SharedPeriod
        /// will return a DateTimePeriod where the start and the end time are equal. 
        /// </remarks>
        public DateTimePeriod? Intersection(DateTimePeriod intersectPeriod)
        {
            if (!Intersect(intersectPeriod))
                return null;

            DateTime intersectStart = intersectPeriod.StartDateTime;
            DateTime intersectEnd = intersectPeriod.EndDateTime;
            DateTime start = this.period.Minimum;
            if (intersectStart > start)
                start = intersectStart;
            DateTime end = this.period.Maximum;
            if (intersectEnd < end)
                end = intersectEnd;
            return new DateTimePeriod(start, end, false);
        }

        /// <summary>
        /// Returns if the specified periods are adjacent, so the end date is equal to the others start date.
        /// </summary>
        /// <param name="adjacentPeriod">The period.</param>
        /// <returns></returns>
        public bool Adjacent(DateTimePeriod adjacentPeriod)
        {
            return (StartDateTime == adjacentPeriod.EndDateTime || EndDateTime == adjacentPeriod.StartDateTime);
        }

        /// <summary>
        /// Returns whether the specified periods are adjacent within a specified offset. That is the gap, or the intersect between the
        /// first period end date and the others period start date is within the offset. As an example, if period A ends at 22:30 and 
        /// period B starts at 23:00, they are Adjacent if the offset is more than 30 minutes. Also adjacent, if period B starts at 22:00.
        /// </summary>
        /// <param name="adjacentPeriod">The period.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public bool Adjacent(DateTimePeriod adjacentPeriod, TimeSpan offset)
        {
            return (StartDateTime.Subtract(adjacentPeriod.EndDateTime).Duration() <= offset || EndDateTime.Subtract(adjacentPeriod.StartDateTime).Duration() <= offset);
        }

        //public IList<DateTimePeriod> Union(IList<DateTimePeriod> periodList)
        //{
        //    return null;
        //}
 
        /// <summary>
        /// Gets the time period representing the union of the two periods.
        /// If the periods does not intersect or adjacent then null is returned
        /// </summary>
        /// <param name="unionPeriod">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2008-05-16
        /// </remarks>
        public DateTimePeriod? Union(DateTimePeriod unionPeriod)
        {
            if (Intersect(unionPeriod) || Adjacent(unionPeriod))
            {
                DateTime start = earliest(StartDateTime, unionPeriod.StartDateTime);
                DateTime end = latest(EndDateTime, unionPeriod.EndDateTime);
                return new DateTimePeriod(start, end, false);
            }
            return null;
        }


        /// <summary>
        /// Gets the time period representing the union of the two periods.
        /// If the periods does not intersect or adjacent withing the offset, then null is returned
        /// </summary>
        /// <param name="unionPeriod">The period.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2008-05-16
        /// </remarks>
        public DateTimePeriod? Union(DateTimePeriod unionPeriod, TimeSpan offset)
        {
            if (Intersect(unionPeriod) || Adjacent(unionPeriod, offset))
            {
                DateTime start = earliest(StartDateTime, unionPeriod.StartDateTime);
                DateTime end = latest(EndDateTime, unionPeriod.EndDateTime);
                return new DateTimePeriod(start, end, false);
            }
            return null;
        }


        private static DateTime earliest(DateTime first, DateTime second)
        {
            return first <= second ? first : second;
        }

        private static DateTime latest(DateTime first, DateTime second)
        {
            return first >= second ? first : second;
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        /// <remarks>
        /// Enables easier testing and debugging
        /// Created by: rogerkr
        /// Created date: 2008-02-11
        /// </remarks>
        public override string ToString()
        {
            return StartDateTime + DATETIME_SEPARATOR + EndDateTime;
        }

        /// <summary>
        /// Toes the local string.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-03-06
        /// </remarks>
        public string ToLocalString()
        {
            return LocalStartDateTime + DATETIME_SEPARATOR + LocalEndDateTime;
        }

        /// <summary>
        /// Converts to DateOnlyPeriod using the timezone sent as parameter.
        /// </summary>
        /// <param name="info">The time zone info.</param>
        /// <returns></returns>
        public DateOnlyPeriod ToDateOnlyPeriod(ICccTimeZoneInfo info)
        {
            var localStartDate = TimeZoneHelper.ConvertFromUtc(period.Minimum, info);
            var localEndDate = TimeZoneHelper.ConvertFromUtc(period.Maximum, info);
            if (localEndDate.TimeOfDay == TimeSpan.Zero)
                localEndDate = localEndDate.AddHours(-1);
            if (localEndDate < localStartDate)
                localEndDate = localStartDate;
            return new DateOnlyPeriod(new DateOnly(localStartDate), new DateOnly(localEndDate));
        }

        /// <summary>
        /// Gets the local date string.
        /// </summary>
        /// <value>The local date string.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-03-06
        /// </remarks>
        public string LocalDateString
        {
            get
            {
                return
                    LocalStartDateTime.ToShortDateString() + DATETIME_SEPARATOR + LocalEndDateTime.AddDays(-1).ToShortDateString();
            }
        }

        /// <summary>
        /// The local times as time period.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-18
        /// </remarks>
        public TimePeriod TimePeriodLocal()
        {
            return TimePeriod(TimeZoneHelper.CurrentSessionTimeZone);
        }

        /// <summary>
        /// Get the time period
        /// </summary>
        /// <param name="timeZone">The time zone.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-18
        /// </remarks>
        public TimePeriod TimePeriod(ICccTimeZoneInfo timeZone)
        {
            TimeSpan startTimeOfDay;
            if (timeZone.Id == timeZone.Utc.Id)
                startTimeOfDay = StartDateTime.TimeOfDay;
            else
                startTimeOfDay = timeZone.ConvertTimeFromUtc(StartDateTime, timeZone).TimeOfDay;

            return new TimePeriod(startTimeOfDay, startTimeOfDay.Add(ElapsedTime()));
        }

        /// <summary>
        /// Gets a new period starting the first day of the week
        /// ending the last day of week
        /// (in sweden: startdate will back to nearest monday, enddate will be forward to closest sunday)
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-14
        /// </remarks>
        /// <param name="timeZoneInfo"></param>
        public DateTimePeriod WholeWeek(CultureInfo culture, ICccTimeZoneInfo timeZoneInfo)
        {
            DateTime startdate = DateHelper.GetFirstDateInWeek(StartDateTimeLocal(timeZoneInfo), culture);
            DateTime endDate = DateHelper.GetLastDateInWeek(EndDateTimeLocal(timeZoneInfo), culture).AddDays(1);
            return new DateTimePeriod(timeZoneInfo.ConvertTimeToUtc(startdate, timeZoneInfo),
                                      timeZoneInfo.ConvertTimeToUtc(endDate, timeZoneInfo), false);
        }

        /// <summary>
        /// Get the maximum period.
        /// A new DateTimePeriod is returned with the lowest
        /// start DateTime and the highest end DateTime.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-20
        /// </remarks>
        public DateTimePeriod MaximumPeriod(DateTimePeriod dateTimePeriod)
        {
            DateTime lowDate = StartDateTime;
            DateTime highDate = EndDateTime;
            if (dateTimePeriod.StartDateTime < lowDate)
                lowDate = dateTimePeriod.StartDateTime;
            if (dateTimePeriod.EndDateTime > highDate)
                highDate = dateTimePeriod.EndDateTime;
            return new DateTimePeriod(lowDate, highDate, false);
        }

        /// <summary>
        /// Get the maximum period. If one of the periods are null,
        /// the other is returned. If both are null, null is returned.
        /// </summary>
        /// <param name="period1">The period1.</param>
        /// <param name="period2">The period2.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-02-06
        /// </remarks>
        public static DateTimePeriod? MaximumPeriod(DateTimePeriod? period1, DateTimePeriod? period2)
        {
            if (period1.HasValue && period2.HasValue)
                return period1.Value.MaximumPeriod(period2.Value);
            if (period1.HasValue)
                return period1.Value;
            if (period2.HasValue)
                return period2.Value;
            return null;
        }

        /// <summary>
        /// Returns a list of periods splitted into the length of the interval
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-12-05
        /// </remarks>
        public IList<DateTimePeriod> Intervals(TimeSpan interval)
        {
            IList<DateTimePeriod> retList = new List<DateTimePeriod>();
            DateTime intervalStart= StartDateTime;
            
            while (intervalStart<EndDateTime)
            {
                DateTime intervalEnd = intervalStart.Add(interval);
                retList.Add(new DateTimePeriod(intervalStart,intervalEnd,false));
                intervalStart = intervalEnd;
            }
            return retList;
        }

        /// <summary>
        /// Merges two lists of DataTimePeriods into one.
        /// </summary>
        /// <param name="list1">The list1.</param>
        /// <param name="list2">The list2.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke, algorithm by Tamas
        /// Created date: 2009-01-26
        /// </remarks>
        public static IList<DateTimePeriod> MergeLists(IList<DateTimePeriod> list1, IList<DateTimePeriod> list2)
        {
            IList<DateTimePeriod> ret = new List<DateTimePeriod>();

            if(list1.Count == 0)
                return list2;

            if(list2.Count == 0)
                return list1;

            List<DateTime> startTimes = new List<DateTime>();
            List<DateTime> endTimes = new List<DateTime>();

            foreach (var dateTimePeriod in list1)
            {
                startTimes.Add(dateTimePeriod.StartDateTime);
                endTimes.Add(dateTimePeriod.EndDateTime);
            }

            foreach (var dateTimePeriod in list2)
            {
                startTimes.Add(dateTimePeriod.StartDateTime);
                endTimes.Add(dateTimePeriod.EndDateTime);
            }

            startTimes.Sort();
            endTimes.Sort();
            endTimes.Insert(0, DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc));

            IList<DateTime> filteredStartTimes = new List<DateTime>();
            IList<DateTime> filteredEndTimes = new List<DateTime>();
            filteredStartTimes.Add(startTimes[0]);
            filteredEndTimes.Add(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc));
            for (int index = 1; index < startTimes.Count; index++)
            {
                if (startTimes[index] > endTimes[index])
                {
                    filteredStartTimes.Add(startTimes[index]);
                    filteredEndTimes.Add(endTimes[index]);
                }
            }

            filteredEndTimes.Add(endTimes[endTimes.Count - 1]);
            filteredEndTimes.RemoveAt(0);

            for (int index = 0; index < filteredEndTimes.Count; index++)
            {
                ret.Add(new DateTimePeriod(filteredStartTimes[index], filteredEndTimes[index], false));
            }

            return ret;
        }

        /// <summary>
        /// Exclude a date time period from this period
        /// </summary>
        /// <param name="periodUpToExclude">period up to be excluded</param>
        /// <returns>Rest periods</returns>
        public IEnumerable<DateTimePeriod> ExcludeDateTimePeriod(DateTimePeriod periodUpToExclude)
        {
            var timePeriods = new List<DateTimePeriod>();
            if (StartDateTime < periodUpToExclude.StartDateTime)
            {
                var leftTimePeriod = new DateTimePeriod(StartDateTime,
                                                        periodUpToExclude.StartDateTime);
                timePeriods.Add(leftTimePeriod);
            }
            if (EndDateTime > periodUpToExclude.EndDateTime)
            {
                var rightTimePeriod = new DateTimePeriod(periodUpToExclude.EndDateTime,
                                                         EndDateTime);
                timePeriods.Add(rightTimePeriod);
            }
            return timePeriods;
        }

        /// <summary>
        /// Returns the elapsed time TotalDays rounded to an integer.
        /// </summary>
        /// <returns></returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-02-17    
        /// /// </remarks>
        public double RoundedDaysCount()
        {
            TimeSpan elapsedTime = period.Maximum - period.Minimum;
            return Math.Round(elapsedTime.TotalDays);
        }
    }
}