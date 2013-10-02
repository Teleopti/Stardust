using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public static class DayIntervalGenerator
    {
        public static IList<TimeSpan> IntervalForTwoDays(int resolution)
        {
            var intervalsOfTwoDays = new List<TimeSpan>();
            for (var i = 0; i < TimeSpan.FromDays(2).TotalMinutes / resolution; i++)
            {
                intervalsOfTwoDays.Add(TimeSpan.FromMinutes(i * resolution));
            }
            return intervalsOfTwoDays;
        }

        public static List<TimeSpan> IntervalForFirstDay(int resolution)
        {
            return getIntervals(resolution,0);
        }

        public static List<TimeSpan> IntervalForSecondDay(int resolution)
        {
            return getIntervals(resolution,1);
        }

        private static List<TimeSpan> getIntervals(int resolution, int dayIndex)
        {
            var intervalsOfTwoDays = new List<TimeSpan>();
            for (var i = 0; i < TimeSpan.FromDays(1).TotalMinutes / resolution; i++)
            {
                var timeToAdd = TimeSpan.FromMinutes(i * resolution).Add(TimeSpan.FromDays(dayIndex));
                intervalsOfTwoDays.Add(timeToAdd);
            }
            return intervalsOfTwoDays;
        }
    }
}