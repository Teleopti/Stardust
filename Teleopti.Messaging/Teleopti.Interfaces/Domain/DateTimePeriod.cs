using System;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.Domain
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
	}
}