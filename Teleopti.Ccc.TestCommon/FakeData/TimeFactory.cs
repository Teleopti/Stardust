using System;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for Time related domain objects
    /// </summary>
    public static class TimeFactory
    {
        /// <summary>
        /// Creates the time period.
        /// </summary>
        /// <param name="startHour">The start hour.</param>
        /// <param name="endHour">The end hour.</param>
        /// <returns></returns>
        public static TimePeriod CreateTimePeriod(int startHour, int endHour)
        {
            return new TimePeriod(CreateTimeSpan(startHour), CreateTimeSpan(endHour));
        }

        /// <summary>
        /// Creates the time span.
        /// </summary>
        /// <param name="hour">The hour.</param>
        /// <returns></returns>
        public static TimeSpan CreateTimeSpan(int hour)
        {
            return new TimeSpan(hour, 0, 0);
        }

        /// <summary>
        /// Creates the anchored time period.
        /// </summary>
        /// <param name="anchorHours">The center position (from start of interval)</param>
        /// <param name="durationHours">The length of time period</param>
        /// <returns>AnchorTimePeriod</returns>
        public static AnchorTimePeriod CreateAnchorTimePeriod(int anchorHours, int durationHours)
        {
            return new AnchorTimePeriod(CreateTimeSpan(anchorHours), CreateTimeSpan(durationHours), new Percent(0.30d));
        }

        /// <summary>
        /// Creates an anhored date time period
        /// </summary>
        /// <param name="intervalStart">Date when interval starts</param>
        /// <param name="anchorHours">Time for anchor relative to interval start</param>
        /// <param name="durationHours">Length of period</param>
        /// <returns>AnchorDateTimePeriod</returns>
        //public static DayOff CreateAnchorDateTimePeriod(DateTime intervalStart, int anchorHours,
        //                                                              int durationHours)
        //{
        //    intervalStart = DateTime.SpecifyKind(intervalStart, DateTimeKind.Utc);
        //    return CreateAnchorTimePeriod(anchorHours, durationHours).GetAnchorDateTimePeriod(intervalStart);
        //}
    }
}