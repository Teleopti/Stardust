using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Struct for storage of time zone independent date
	/// </summary>
	[Serializable]
	[DataContract]
	public struct DateOnly : IComparable<DateOnly>, IEquatable<DateOnly>, IComparable
	{
		private DateTime _internalDateTime;

		/// <summary>
		/// Initializes a new instance of the <see cref="DateOnly"/> struct.
		/// </summary>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public DateOnly(int year, int month, int day)
		{
			_internalDateTime = new DateTime(year, month, day, 0, 0, 0, 0, DateTimeKind.Unspecified);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DateOnly"/> struct.
		/// </summary>
		/// <param name="dateTime">The date time.</param>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public DateOnly(DateTime dateTime)
			: this(dateTime.Year, dateTime.Month, dateTime.Day)
		{

		}

		/// <summary>
		/// Gets the year.
		/// </summary>
		/// <value>The year.</value>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public int Year => _internalDateTime.Year;

		/// <summary>
		/// Gets the month.
		/// </summary>
		/// <value>The month.</value>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public int Month => _internalDateTime.Month;

		/// <summary>
		/// Gets the day.
		/// </summary>
		/// <value>The day.</value>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public int Day => _internalDateTime.Day;

		/// <summary>
		/// Days the of week.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public DayOfWeek DayOfWeek => _internalDateTime.DayOfWeek;

		/// <summary>
		/// Gets the date.
		/// </summary>
		/// <value>The date.</value>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		[DataMember]
		public DateTime Date
		{
			get { return _internalDateTime.Date; }
			set { _internalDateTime = new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, 0, value.Kind); }
		}

		public TimeSpan Subtract(DateOnly other)
		{
			return _internalDateTime.Subtract(other._internalDateTime);
		}

		public DateOnly AddMonths(Calendar calendar, int months)
		{
			return new DateOnly(calendar.AddMonths(_internalDateTime, months));
		}

		/// <summary>
		/// Gets the max value.
		/// </summary>
		/// <value>The max value.</value>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public static DateOnly MaxValue => new DateOnly(DateHelper.MaxSmallDateTime);

		/// <summary>
		/// Gets the min value.
		/// </summary>
		/// <value>The min value.</value>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public static DateOnly MinValue => new DateOnly(DateHelper.MinSmallDateTime);

		/// <summary>
		/// Gets the today. HA HA !!1
		/// </summary>
		/// <value>The today.</value>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2010-04-20
		/// </remarks>
		public static DateOnly Today => new DateOnly(DateTime.Today);

		#region operators

		/// <summary>
		/// Implements the operator &lt;.
		/// </summary>
		/// <param name="obj1">The obj1.</param>
		/// <param name="obj2">The obj2.</param>
		/// <returns>The result of the operator.</returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public static bool operator <(DateOnly obj1, DateOnly obj2)
		{
			return obj1.Date < obj2.Date;
		}

		/// <summary>
		/// Implements the operator &lt;=.
		/// </summary>
		/// <param name="obj1">The obj1.</param>
		/// <param name="obj2">The obj2.</param>
		/// <returns>The result of the operator.</returns>
		/// <remarks>
		///  Created by: Ola
		///  Created date: 2009-03-18    
		/// </remarks>
		public static bool operator <=(DateOnly obj1, DateOnly obj2)
		{
			return obj1.Date <= obj2.Date;
		}

		/// <summary>
		/// Implements the operator &gt;=.
		/// </summary>
		/// <param name="obj1">The obj1.</param>
		/// <param name="obj2">The obj2.</param>
		/// <returns>The result of the operator.</returns>
		/// <remarks>
		///  Created by: Ola
		///  Created date: 2009-03-18    
		/// </remarks>
		public static bool operator >=(DateOnly obj1, DateOnly obj2)
		{
			return obj1.Date >= obj2.Date;
		}

		/// <summary>
		/// Implements the operator &gt;.
		/// </summary>
		/// <param name="obj1">The obj1.</param>
		/// <param name="obj2">The obj2.</param>
		/// <returns>The result of the operator.</returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public static bool operator >(DateOnly obj1, DateOnly obj2)
		{
			return obj1.Date > obj2.Date;
		}
		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="obj1">The obj1.</param>
		/// <param name="obj2">The obj2.</param>
		/// <returns>The result of the operator.</returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public static bool operator ==(DateOnly obj1, DateOnly obj2)
		{
			return obj1.Equals(obj2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="obj1">The obj1.</param>
		/// <param name="obj2">The obj2.</param>
		/// <returns>The result of the operator.</returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public static bool operator !=(DateOnly obj1, DateOnly obj2)
		{
			return !obj1.Equals(obj2);
		}

		#endregion

		#region Equals and GetHashCode stuff

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public override bool Equals(object obj)
		{
			return obj is DateOnly only && Equals(only);
		}

		/// <summary>
		/// Equalses the specified other.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public bool Equals(DateOnly other)
		{
			return (_internalDateTime.Equals(other._internalDateTime));
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public override int GetHashCode()
		{
			return _internalDateTime.GetHashCode();
		}

		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings:
		/// Value
		/// Meaning
		/// Less than zero
		/// This instance is less than <paramref name="obj"/>.
		/// Zero
		/// This instance is equal to <paramref name="obj"/>.
		/// Greater than zero
		/// This instance is greater than <paramref name="obj"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="obj"/> is not the same type as this instance.
		/// </exception>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public int CompareTo(object obj)
		{
			return CompareTo((DateOnly)obj);
		}

		/// <summary>
		/// Compares to.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public int CompareTo(DateOnly other)
		{

			return _internalDateTime.CompareTo(other.Date);
		}

		#endregion


		/// <summary>
		/// Adds the days.
		/// </summary>
		/// <param name="days">The days.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public DateOnly AddDays(int days)
		{
			return new DateOnly(_internalDateTime.AddDays(days));
		}

		public DateOnly AddWeeks(int weeks)
		{
			return AddDays(7*weeks);
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-18
		/// </remarks>
		public override string ToString()
		{
			return _internalDateTime.ToString();
		}

		/// <summary>
		/// Toes the short date string.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2009-03-21
		/// </remarks>
		public string ToShortDateString()
		{
			return _internalDateTime.ToShortDateString();
		}

		/// <summary>
		/// Toes the short date string using the given culture.
		/// </summary>
		/// <param name="cultureInfo">The culture info.</param>
		/// <returns></returns>
		/// /// <remarks>
		/// Created by: jonas n
		/// Created date: 2010-11-23
		/// </remarks>
		public string ToShortDateString(IFormatProvider cultureInfo)
		{
			return _internalDateTime.ToString("d", cultureInfo);
		}

		public DateOnly Add(TimeSpan timeSpan)
		{
			return new DateOnly(_internalDateTime.Add(timeSpan));
		}

		public DateTimePeriod ToDateTimePeriod(TimeZoneInfo timeZoneInfo)
		{
			return new DateTimePeriod(timeZoneInfo.SafeConvertTimeToUtc(Date),
									  timeZoneInfo.SafeConvertTimeToUtc(Date.AddDays(1).Date));
		}

		public DateTimePeriod ToDateTimePeriod(TimePeriod period, TimeZoneInfo timeZoneInfo)
		{
			var dateTime = TimeZoneHelper.ConvertToUtc(Date, timeZoneInfo);
			return new DateTimePeriod(dateTime.Add(period.StartTime), dateTime.Add(period.EndTime));
		}

		public DateOnlyPeriod ToDateOnlyPeriod()
		{
			return new DateOnlyPeriod(this, this);
		}
	}
}
