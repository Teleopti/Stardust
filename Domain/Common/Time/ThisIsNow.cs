using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public sealed class ThisIsNow : INow
	{
		private readonly DateTime _utcTheTime;

		public ThisIsNow(string timeInUtc)
			: this(timeInUtc.ToTime())
		{
		}

		public ThisIsNow(DateTime utcTheTime)
		{
			_utcTheTime = utcTheTime;
		}

		public DateTime UtcDateTime()
		{
			return _utcTheTime;
		}

		public bool IsExplicitlySet()
		{
			return true;
		}
	}

	public static class StringExtensions
	{
		public static DateTime ToTime(this string dateTimeString)
		{
			return DateTime.Parse(dateTimeString + "Z", CultureInfo.GetCultureInfo("sv-SE"));
		}
	}
}