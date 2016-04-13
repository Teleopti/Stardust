using System;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public static class IntervalHelper
	{
		public static DateTime GetValidIntervalTime(int intervalLength, DateTime time)
		{
			double minutes = time.TimeOfDay.Minutes;
			var remainder = minutes % intervalLength;

			return time.Subtract(TimeSpan.FromMinutes(remainder));
		}

		public static int GetIntervalId(int intervalLength, DateTime time)
		{
			var minutesElapsedOfDay = time.TimeOfDay.TotalMinutes;
			var id = (int)minutesElapsedOfDay / intervalLength;

			return id;
		}
	}
}