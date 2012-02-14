using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Schedules
{
    public class PixelConverter
    {
        private readonly TimePeriod _period;
        private readonly double _pixelPerMinute;
        private readonly bool _rightToLeft;
        private readonly int _width;

        public PixelConverter(int width, TimePeriod period, bool rightToLeft)
        {
            _period = period;
            _width = width;
            _pixelPerMinute = width / period.SpanningTime().TotalMinutes;
            _rightToLeft = rightToLeft;
        }

        public int GetPixelFromTimeSpan(TimeSpan timeSpan)
        {
            int px = ((int)((int)(timeSpan.Subtract(_period.StartTime).TotalMinutes) * _pixelPerMinute));
            if (!_rightToLeft)
                return px;

            return _width - px;

        }

        //public static TimeSpan GetTimeSpanFromPixel(int pixel)
        //{
        //    throw new NotImplementedException();
        //}
    }
}