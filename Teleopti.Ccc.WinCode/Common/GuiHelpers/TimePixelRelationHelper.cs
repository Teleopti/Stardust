using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.GuiHelpers
{
    /// <summary>
    /// Handles scaling of time and pixels.
    /// </summary>
    public class TimePixelRelationHelper
    {
        private int _width;
        private TimePeriod _period;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePixelRelationHelper"/> class.
        /// </summary>
        /// <param name="width">The width of the container, in pixels.</param>
        /// <param name="period">The period that should be viewed in the container.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 9.12.2007
        /// </remarks>
        public TimePixelRelationHelper(int width, TimePeriod period)
        {
            _width = width;
            _period = period;
        }

        /// <summary>
        /// Gets the minutes per pixel.
        /// </summary>
        /// <value>The minutes per pixel.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 9.12.2007
        /// </remarks>
        public double MinutesPerPixel()
        {
            return _period.SpanningTime().TotalMinutes/_width;
        }

        /// <summary>
        /// Pixels from minute.
        /// </summary>
        /// <param name="minutes">The minute.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 10.12.2007
        /// </remarks>
        public int PixelFromTime(int minutes)
        {
            ValidatePeriod(minutes);
            return (int)Math.Round(OffsetMinutes(minutes) / MinutesPerPixel(), 0);
        }

        /// <summary>
        /// Pixels from time span.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 10.12.2007
        /// </remarks>
        public int PixelFromTime(TimeSpan timeSpan)
        {
            return PixelFromTime((int)timeSpan.TotalMinutes);
        }

        private int OffsetMinutes(int minutes)
        {
            return minutes - (int) _period.StartTime.TotalMinutes;
        }

        private void ValidatePeriod(int minutes)
        {
            if (minutes<_period.StartTime.TotalMinutes || minutes>_period.EndTime.TotalMinutes)
                throw new ArgumentException("{0} has exceeded the limit of {1} hours.");
        }
    }
}
