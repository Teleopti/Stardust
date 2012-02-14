using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
    public static class TimeZoneFactory
    {
        public static IList<TimeZoneDim> CreateTimeZoneDimList(IEnumerable<TimeZoneInfo> timeZoneCollection, TimeZoneInfo defaultTimeZone)
        {
            IList<TimeZoneDim> retList = new List<TimeZoneDim>();
            //ReadOnlyCollection<TimeZoneInfo> systemTimeZoneCollection = TimeZoneInfo.GetSystemTimeZones();

            foreach (TimeZoneInfo timeZoneInfo in timeZoneCollection)
            {
                retList.Add(new TimeZoneDim(timeZoneInfo, defaultTimeZone));
            }
            return retList;
        }

        public static IList<TimeZoneBridge> CreateTimeZoneBridgeList(DateTimePeriod period, int intervalsPerDay, IEnumerable<TimeZoneInfo> timeZoneInfoList)
        {
            IList<TimeZoneBridge> retList = new List<TimeZoneBridge>();
            
            int minutesPerInterval = 1440 / intervalsPerDay;
            DateTime startDate = GetNearestLowerIntervalTime(period.StartDateTime, minutesPerInterval);
            DateTime endDate = GetNearestLowerIntervalTime(period.EndDateTime, minutesPerInterval);
            //DateTime endDate = period.EndDateTime.Date.AddDays(1);
            DateTime currentDateTime = startDate;

            while (currentDateTime <= endDate)
            {
                foreach (TimeZoneInfo zone in timeZoneInfoList)
                {
                    TimeZoneBridge timeZoneBridge =
                        new TimeZoneBridge(currentDateTime, zone, intervalsPerDay);
                    if (timeZoneBridge.Date > DateTime.MinValue)
                        retList.Add(timeZoneBridge);
                }

                currentDateTime = currentDateTime.AddMinutes(minutesPerInterval);
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