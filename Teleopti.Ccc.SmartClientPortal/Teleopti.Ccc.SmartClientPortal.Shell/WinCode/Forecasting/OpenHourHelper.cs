using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting
{
    /// <summary>
    /// Helper for OpenHour handling
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2007-12-18
    /// </remarks>
    public static class OpenHourHelper
    {
        /// <summary>
        /// Checks the openHour string.
        /// </summary>
        /// <param name="value">The open hour string.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-29
        /// </remarks>
        public static bool CheckOpenHourString(string value, out TimeSpan start, out TimeSpan end)
        {
            string[] sae = value.Split(new[] { "-" }, StringSplitOptions.None);

            if (sae.Length == 2)
            {
                end = TimeSpan.Zero;
                return (TimeHelper.TryParse(sae[0], out start) &&
                        TimeHelper.TryParse(sae[1], out end));
            }

            start = TimeSpan.Zero;
            end = TimeSpan.Zero;
            return false;
        }

        /// <summary>
        /// Checks if end time is less than start time. If so it adds 1 day to end time
        /// </summary>
        /// <param name="value">The open hour string.</param>
        /// <param name="midnightBreakOffset"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-30
        /// </remarks>
        public static TimePeriod OpenHourPeriod(string value, TimeSpan midnightBreakOffset)
        {
            TimeSpan end;
            TimeSpan start;
            TimePeriod openHour;
            bool dayAfter = false;
            if (value.Contains("+"))
            {
                value = value.Substring(0, value.IndexOf('+'));
                dayAfter = true;
            }

            if (CheckOpenHourString(value, out start, out end))
            {
                if (start < midnightBreakOffset)
                    start = start.Add(TimeSpan.FromDays(1));
                if (start >= end || dayAfter)
                {
                    openHour = new TimePeriod(start, end.Add(TimeSpan.FromDays(1)));
                }
                else
                {
                    openHour = new TimePeriod(start, end);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("value", "Something is wrong with the given times");
            }
            return openHour;
        }

        /// <summary>
        /// Gets the open hour from string.
        /// It will fit to default resolution and add 1 day if end time is less than start time.
        /// </summary>
        /// <param name="value">The open hour string.</param>
        /// <param name="defaultResolution">The default resolution.</param>
        /// <param name="midnightOffsetBreak">Midnight offset break</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-05
        /// </remarks>
        public static TimePeriod OpenHourFromString(string value, int defaultResolution, TimeSpan midnightOffsetBreak)
        {
            TimePeriod openHour = OpenHourPeriod(value, midnightOffsetBreak);
            TimeSpan start = openHour.StartTime;
            TimeSpan end = openHour.EndTime;
            start = TimeHelper.FitToDefaultResolution(start, defaultResolution);
            end = TimeHelper.FitToDefaultResolution(end, defaultResolution);
            openHour = new TimePeriod(start, end);
            return openHour;
        }
    }
}
