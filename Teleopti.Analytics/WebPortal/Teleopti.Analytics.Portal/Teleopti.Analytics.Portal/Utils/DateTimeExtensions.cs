using System;

namespace Teleopti.Analytics.Portal.Utils
{
	public static class DateTimeExtensions
	{
		public static bool IsEarlierThan(this DateTime dateTime, DateTime dateToCompareWith)
		{
			return dateTime.CompareTo(dateToCompareWith) < 0;
		}

		public static bool IsLaterThan(this DateTime dateTime, DateTime dateToCompareWith)
		{
			return dateTime.CompareTo(dateToCompareWith) > 0;
		}
	}
}