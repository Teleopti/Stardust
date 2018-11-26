using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Wfm.Adherence
{
	public static class DateOnlyExtensions
	{
		public static IEnumerable<DateOnly> DateRange(this DateOnly instance, int days)
		{
			return from i in Enumerable.Range(0, days) select instance.AddDays(i);
		}
	}

	public struct DateOnly : IComparable<DateOnly>, IEquatable<DateOnly>, IComparable
	{
		private DateTime _internalDateTime;

		public DateOnly(DateTime dateTime)
		{
			_internalDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0, DateTimeKind.Unspecified);
		}

		public DateTime Date
		{
			get => _internalDateTime.Date;
			set => _internalDateTime = new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, 0, value.Kind);
		}

		public static bool operator <(DateOnly obj1, DateOnly obj2)
		{
			return obj1.Date < obj2.Date;
		}

		public static bool operator <=(DateOnly obj1, DateOnly obj2)
		{
			return obj1.Date <= obj2.Date;
		}

		public static bool operator >=(DateOnly obj1, DateOnly obj2)
		{
			return obj1.Date >= obj2.Date;
		}

		public static bool operator >(DateOnly obj1, DateOnly obj2)
		{
			return obj1.Date > obj2.Date;
		}

		public static bool operator ==(DateOnly obj1, DateOnly obj2)
		{
			return obj1.Equals(obj2);
		}

		public static bool operator !=(DateOnly obj1, DateOnly obj2)
		{
			return !obj1.Equals(obj2);
		}

		public override bool Equals(object obj)
		{
			return obj is DateOnly only && Equals(only);
		}

		public bool Equals(DateOnly other)
		{
			return (_internalDateTime.Equals(other._internalDateTime));
		}

		public override int GetHashCode()
		{
			return _internalDateTime.GetHashCode();
		}

		public int CompareTo(object obj)
		{
			return CompareTo((DateOnly) obj);
		}

		public int CompareTo(DateOnly other)
		{
			return _internalDateTime.CompareTo(other.Date);
		}

		public DateOnly AddDays(int days)
		{
			return new DateOnly(_internalDateTime.AddDays(days));
		}

		public override string ToString()
		{
			return _internalDateTime.ToString();
		}
	}
}