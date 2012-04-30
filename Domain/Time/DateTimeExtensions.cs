using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Rounds of to nearest interval
        /// </summary>
        /// <param name="resolutionInMinutes">The resolution in minutes.</param>
        /// <returns></returns>
        public static DateTime ToInterval(this DateTime dateTime, int resolutionInMinutes)
        {
            return dateTime.ToInterval(resolutionInMinutes, IntervalRounding.Nearest);
        }

        /// <summary>
        /// Rounds of to nearest interval
        /// </summary>
        /// <param name="interval">The interval.</param>
        public static DateTime ToInterval(this DateTime dateTime,TimeSpan interval)
        {
            return dateTime.ToInterval((int) interval.TotalMinutes, IntervalRounding.Nearest);
        }

        /// <summary>
        /// Rounds of to specified interval (Up/Down/Nearest)
        /// </summary>
        /// <param name="resolutionInMinutes">The resolution in minutes.</param>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        public static DateTime ToInterval(this DateTime dateTime, int resolutionInMinutes, IntervalRounding mode)
        {
            InParameter.ValueMustBeLargerThanZero("resolutionInMinutes", resolutionInMinutes);
            double factor = (double)dateTime.Minute / resolutionInMinutes;
            return dateTime.Date.AddHours(dateTime.Hour).AddMinutes(resolutionInMinutes * NumberOfIntervals(factor, mode));
        }

        private static int NumberOfIntervals(double factor,IntervalRounding mode)
        {
            
            switch (mode)
            {
                case IntervalRounding.Down:
                    return (int) factor;
                   
                case IntervalRounding.Up:
                    return (int) Math.Ceiling(factor);
                   
                default:
                    return (int) Math.Round(factor, MidpointRounding.AwayFromZero);
            }
        }

        public static string ToShortTimeStringWithDays(this DateTime dateTime)
        {
            var days = dateTime.Subtract(DateTime.MinValue).Days;
            return dateTime.ToShortTimeString() + (days == 0 ? string.Empty : "+" + days);
        }
    }
}
