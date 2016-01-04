using System;

namespace Teleopti.Interfaces.Domain
{
	public struct HourSlot : IComparable<HourSlot>, IEquatable<HourSlot>, IComparable
	{
		private DateTime _internalDateTime;

		public HourSlot(int year, int month, int day, int hour)
		{
			_internalDateTime = new DateTime(year, month, day, hour, 0, 0, 0, DateTimeKind.Unspecified);
		}

		public HourSlot(DateTime dateTime)
			: this(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour)
		{

		}

		public int Year
		{
			get { return _internalDateTime.Year; }
		}

		public int Month
		{
			get { return _internalDateTime.Month; }
		}

		public int Day
		{
			get { return _internalDateTime.Day; }
		}

		public int Hour
		{
			get { return _internalDateTime.Hour; }
		}

		public DateTime AsDateTime
		{
			get { return _internalDateTime; }
		}

		public static HourSlot Current
		{
			get { return new HourSlot(DateTime.UtcNow); }
		}

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
		public static bool operator <(HourSlot obj1, HourSlot obj2)
		{
			return obj1._internalDateTime < obj2._internalDateTime;
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
		public static bool operator <=(HourSlot obj1, HourSlot obj2)
		{
			return obj1._internalDateTime <= obj2._internalDateTime;
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
		public static bool operator >=(HourSlot obj1, HourSlot obj2)
		{
			return obj1._internalDateTime >= obj2._internalDateTime;
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
		public static bool operator >(HourSlot obj1, HourSlot obj2)
		{
			return obj1._internalDateTime > obj2._internalDateTime;
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
		public static bool operator ==(HourSlot obj1, HourSlot obj2)
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
		public static bool operator !=(HourSlot obj1, HourSlot obj2)
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
			if (obj == null)
				return false;

			if (obj is HourSlot)
				return Equals((HourSlot)obj);

			return false;
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
		public bool Equals(HourSlot other)
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
			return CompareTo((HourSlot)obj);
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
		public int CompareTo(HourSlot other)
		{

			return _internalDateTime.CompareTo(other._internalDateTime);
		}

		#endregion

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
	}
}