using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// DateTimePeriod structure that holds information on an action with a certain start and end time in UTC. 
	/// </summary>
	[Serializable]
	public struct DateTimePeriod : IEquatable<DateTimePeriod>
	{
		private MinMax<DateTime> period;
		private const string DATETIME_SEPARATOR = " - ";

		/// <summary>
		/// Gets the start date time in UTC.
		/// </summary>
		/// <value>The start date time.</value>
		public DateTime StartDateTime => period.Minimum;

		/// <summary>
		/// Gets the end date time in UTC.
		/// </summary>
		/// <value>The end date time.</value>
		public DateTime EndDateTime => period.Maximum;

		/// <summary>
		/// Starts the date time local.
		/// </summary>
		/// <param name="timeZoneInfo">The time zone info.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2009-03-19
		/// </remarks>
		public DateTime StartDateTimeLocal(TimeZoneInfo timeZoneInfo)
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
		public DateTime EndDateTimeLocal(TimeZoneInfo timeZoneInfo)
		{
			return TimeZoneHelper.ConvertFromUtc(EndDateTime, timeZoneInfo);
		}

		private DateTimePeriod(DateTime startDateTime, DateTime endDateTime, bool validate)
		{
			if (validate)
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
			InParameter.VerifyDateIsUtc(nameof(startDateTime), startDateTime);
			InParameter.VerifyDateIsUtc(nameof(endDateTime), endDateTime);
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
			int endDay) : this
			(startYear,
				startMonth,
				startDay,
				0,
				endYear,
				endMonth,
				endDay,
				0)
		{
		}

		public DateTimePeriod(int startYear,
			int startMonth,
			int startDay,
			int startHour,
			int endYear,
			int endMonth,
			int endDay,
			int endHour
		)
		{
		    var startDateTimeTemp = new DateTime(startYear, startMonth, startDay, startHour, 0, 0, DateTimeKind.Utc);
		    var endDateTimeTemp = new DateTime(endYear, endMonth, endDay, endHour, 0, 0, DateTimeKind.Utc);

			period = new MinMax<DateTime>(startDateTimeTemp, endDateTimeTemp);
		}

		/// <summary>
		/// Returns a TimeSpan representing the Elapsed time in the TimePeriod.
		/// </summary>
		/// <returns></returns>
		public TimeSpan ElapsedTime()
		{
			return EndDateTime.Subtract(StartDateTime);
		}

		/// <summary>
		/// Convert to local time first, then returns a TimeSpan representing the Elapsed time in the TimePeriod, 
		/// This case for daylight save start and end day
		/// </summary>
		/// <returns></returns>
		public TimeSpan LocalElapsedTime(TimeZoneInfo timeZoneInfo)
		{
			var localStart = StartDateTimeLocal(timeZoneInfo);
			var localEnd = EndDateTimeLocal(timeZoneInfo);

			return checkForEdgeCaseDueToDaylightSavings(localEnd, localStart) ? ElapsedTime() : localEnd.Subtract(localStart);
		}

		/// <summary>
		/// in daylight saving close day, end time may little than start time 
		/// eg: 2:45-2:00 (second 2am)
		/// </summary>
		/// <returns></returns>
		private static bool checkForEdgeCaseDueToDaylightSavings(DateTime localEnd, DateTime localStart)
		{
			return localEnd < localStart;
		}

		/// <summary>
		/// Moves the period the spacific amount of time.
		/// </summary>
		/// <param name="timeSpan">A time span.</param>
		/// <returns></returns>
		public DateTimePeriod MovePeriod(TimeSpan timeSpan)
		{
			return new DateTimePeriod(StartDateTime.Add(timeSpan), EndDateTime.Add(timeSpan), false);
		}

		/// <summary>
		/// Returns if the param DateTimePeriod is contained entirely
		/// </summary>
		/// <param name="containsPeriod">The period.</param>
		/// <returns>
		/// 	<c>true</c> if the specified period is contained; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
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
			return period.Minimum <= date && period.Maximum > date;
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
			return containsPeriod.StartDateTime <= EndDateTime && containsPeriod.EndDateTime >= StartDateTime;
		}

		/// <summary>
		/// Returns if any part of the param DateTime is contained
		/// </summary>
		/// <param name="theDateTime"></param>
		/// <returns><c>true</c> if the specified period is contained; otherwise, <c>false</c>.</returns>
		public bool ContainsPart(DateTime theDateTime)
		{
			InParameter.VerifyDateIsUtc(nameof(theDateTime), theDateTime);

			return theDateTime <= EndDateTime && theDateTime >= StartDateTime;
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
		/// Returns an IList of DateTimePeriods corresponding to each affected hour.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-01-29
		/// </remarks>
		public IList<DateTimePeriod> AffectedHourCollection()
		{
            var endDateTime = EndDateTime.Date.AddHours(EndDateTime.Hour).AddHours(1);
            var startDateTime = StartDateTime.Date.AddHours(StartDateTime.Hour);

			IList<DateTimePeriod> retList = new List<DateTimePeriod>();
			for (; startDateTime < endDateTime; startDateTime = startDateTime.AddHours(1))
			{
				retList.Add(new DateTimePeriod(startDateTime, startDateTime.AddHours(1)));
			}

			return retList;
		}

		/// <summary>
		/// Whole days collection. Uses the start time of period to return whole days (24 hours) after that. Only last period will differ from 24 hours length.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Robink
		/// Created date: 2007-11-21
		/// </remarks>
		public IList<DateTimePeriod> WholeDayCollection(TimeZoneInfo timeZoneInfo)
		{
			IList<DateTimePeriod> collectionToReturn = new List<DateTimePeriod>();
            var currentDateTime = StartDateTimeLocal(timeZoneInfo );
            var endDateTime = EndDateTimeLocal(timeZoneInfo );
			while (currentDateTime < endDateTime)
			{
                var currentEndDateTime = currentDateTime.AddDays(1);
				if (currentEndDateTime > endDateTime) currentEndDateTime = endDateTime;
				if (endDateTime == currentDateTime) break;

				collectionToReturn.Add(TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(currentDateTime,
					currentEndDateTime, timeZoneInfo));
				currentDateTime = currentEndDateTime;
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
		public IList<IntervalDefinition> IntervalsFromHourCollection(int resolution, TimeZoneInfo timeZoneInfo)
		{
			var elapsedTime = ElapsedTime();
			if (elapsedTime == TimeSpan.Zero) return new List<IntervalDefinition>();

			if (resolution <= 0)
				throw new ArgumentException("resolution to low", nameof(resolution));

			if (StartDateTime.Add(TimeSpan.FromMinutes(resolution)) > EndDateTime)
				throw new ArgumentException("resolution to high", nameof(resolution));

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
					TimeZoneHelper.ConvertFromUtc(currentTime, timeZoneInfo).Subtract(localStartDate);
				intervals.Add(new IntervalDefinition(currentTime, timeRelativeToStartOfDay));
				currentTime = currentTime.Add(resolutionAsTimeSpan);
			} while (currentTime < EndDateTime);

			return intervals;
		}

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
			return obj is DateTimePeriod timePeriod && Equals(timePeriod);
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
			return (period.Minimum.GetHashCode() * 397) ^ period.Maximum.GetHashCode();
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
			return per1.ElapsedTime() < per2.ElapsedTime();
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
			return per1.ElapsedTime() > per2.ElapsedTime();
		}

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
		public DateTimePeriod? Intersection(DateTimePeriod intersectPeriod)
		{
			if (!Intersect(intersectPeriod))
				return null;

            var intersectStart = intersectPeriod.StartDateTime;
            var intersectEnd = intersectPeriod.EndDateTime;
            var start = period.Minimum;
			if (intersectStart > start)
				start = intersectStart;
            var end = period.Maximum;
			if (intersectEnd < end)
				end = intersectEnd;
			return new DateTimePeriod(start, end, false);
		}

		/// <summary>
		/// Returns if the specified periods are adjacent, so the end date is equal to the others start date.
		/// </summary>
		/// <param name="adjacentPeriod">The period.</param>
		/// <returns></returns>
		public bool AdjacentTo(DateTimePeriod adjacentPeriod)
		{
			return StartDateTime == adjacentPeriod.EndDateTime || EndDateTime == adjacentPeriod.StartDateTime;
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
		/// Converts to DateOnlyPeriod using the timezone sent as parameter.
		/// </summary>
		/// <param name="info">The time zone info.</param>
		/// <returns></returns>
		public DateOnlyPeriod ToDateOnlyPeriod(TimeZoneInfo info)
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
		/// Get the time period
		/// </summary>
		/// <param name="timeZone">The time zone.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-02-18
		/// </remarks>
		public TimePeriod TimePeriod(TimeZoneInfo timeZone)
		{
        	var startTimeOfDay = StartDateTimeLocal(timeZone).TimeOfDay;

			return new TimePeriod(startTimeOfDay, startTimeOfDay.Add(LocalElapsedTime(timeZone)));
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
            var lowDate = StartDateTime;
            var highDate = EndDateTime;
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
            var intervalStart= StartDateTime;

			while (intervalStart < EndDateTime)
			{
                var intervalEnd = intervalStart.Add(interval);
				retList.Add(new DateTimePeriod(intervalStart, intervalEnd, false));
				intervalStart = intervalEnd;
			}

			return retList;
		}

		/// <summary>
		/// Merges a list lists of DataTimePeriods into one.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<DateTimePeriod> MergePeriods(IEnumerable<DateTimePeriod> periods)
		{
			var sortedPeriods = periods.ToArray();
			if (!sortedPeriods.Any())
				return Enumerable.Empty<DateTimePeriod>();

			Array.Sort(sortedPeriods, (p1, p2) =>
			{
				var result = p1.StartDateTime.CompareTo(p2.StartDateTime);
				if (result != 0) return result;

				return p1.EndDateTime.CompareTo(p2.EndDateTime);
			});

			var sortedCount = sortedPeriods.Length;
			var ret = new List<DateTimePeriod>(sortedCount);
	        for (var i = 0; i < sortedCount; i++)
			{
				var startTime = sortedPeriods[i].StartDateTime;
				var endTime = sortedPeriods[i].EndDateTime;

				for (; i < sortedCount; i++)
				{
					if (sortedPeriods[i].StartDateTime > endTime)
					{
						i--;
						break;
					}

					if (sortedPeriods[i].EndDateTime > endTime)
					{
						endTime = sortedPeriods[i].EndDateTime;
					}
				}

				ret.Add(new DateTimePeriod(startTime, endTime, false));
			}

			return ret;
		}

		/// <summary>
		/// Exclude a date time period from this period
		/// </summary>
		/// <param name="exclude">period up to be excluded</param>
		/// <returns>Rest periods</returns>
		public IEnumerable<DateTimePeriod> Subtract(DateTimePeriod exclude)
		{
			if (!Intersect(exclude))
				return new[] {this};
			
			var timePeriods = new List<DateTimePeriod>();
			if (StartDateTime < exclude.StartDateTime)
			{
				var leftTimePeriod = new DateTimePeriod(StartDateTime, exclude.StartDateTime);
				timePeriods.Add(leftTimePeriod);
			}

			if (EndDateTime > exclude.EndDateTime)
			{
				var rightTimePeriod = new DateTimePeriod(exclude.EndDateTime, EndDateTime);
				timePeriods.Add(rightTimePeriod);
			}

			return timePeriods;
		}
	}
}