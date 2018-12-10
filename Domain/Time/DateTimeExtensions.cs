using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    public static class DateTimeExtensions
    {
        public static DateTime ToInterval(this DateTime dateTime,TimeSpan interval)
        {
            return dateTime.ToInterval((int) interval.TotalMinutes, IntervalRounding.Nearest);
        }

        public static DateTime ToInterval(this DateTime dateTime, int resolutionInMinutes, IntervalRounding mode)
        {
            InParameter.ValueMustBeLargerThanZero(nameof(resolutionInMinutes), resolutionInMinutes);
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
            return dateTime.ToShortTimeString() + (days == 0 ? string.Empty : " +" + days);
        }
    }
}
