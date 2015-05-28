using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public static class TimeZoneFactory
	{
		public static IList<TimeZoneDim> CreateTimeZoneDimList(IEnumerable<TimeZoneInfo> timeZoneCollection, TimeZoneInfo defaultTimeZone)
		{
			IList<TimeZoneDim> retList = new List<TimeZoneDim>();

			foreach (TimeZoneInfo timeZoneInfo in timeZoneCollection)
			{
				retList.Add(new TimeZoneDim(timeZoneInfo, defaultTimeZone));
			}
			return retList;
		}

		public static IList<TimeZoneBridge> CreateTimeZoneBridgeList(IList<TimeZonePeriod> timeZonePeriodList, int intervalsPerDay)
		{
			IList<TimeZoneBridge> retList = new List<TimeZoneBridge>();
			int minutesPerInterval = 1440 / intervalsPerDay;

			foreach (var timeZonePeriod in timeZonePeriodList)
			{
				var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZonePeriod.TimeZoneCode);
				DateTime start = GetNearestLowerIntervalTime(timeZonePeriod.PeriodToLoad.StartDateTime, minutesPerInterval);
				DateTime end = GetNearestLowerIntervalTime(timeZonePeriod.PeriodToLoad.EndDateTime, minutesPerInterval);
				DateTime currDateTime = start;

				while (currDateTime <= end)
				{
					var timeZoneBridge = new TimeZoneBridge(currDateTime, timeZone, intervalsPerDay);
					if (timeZoneBridge.Date > DateTime.MinValue)
						retList.Add(timeZoneBridge);

					currDateTime = currDateTime.AddMinutes(minutesPerInterval);
				}
			}

			return retList;
		}

		private static DateTime GetNearestLowerIntervalTime(DateTime date, int minutesPerInterval)
		{
			double minutesElapsedOfDay = date.TimeOfDay.TotalMinutes;

			return date.Subtract(TimeSpan.FromMinutes(minutesElapsedOfDay % minutesPerInterval));
		}
	}
}