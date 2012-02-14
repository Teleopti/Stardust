using System;
using System.Collections.Generic;
using System.Text;

namespace GridTest
{
    public class PixelConverter
    {
        private readonly TimePeriod _period;
        private readonly double _pixelPerMinute;
        private readonly bool _rightToLeft;

        public PixelConverter(int width, TimePeriod period, bool rightToLeft)
        {
            _period = period;
            _pixelPerMinute = width / period.SpanningTime().TotalMinutes;
            _rightToLeft = rightToLeft;
        }

        public int GetPixelFromTimeSpan(TimeSpan timeSpan)
        {
            if (!_rightToLeft)
                return ((int)((int)(timeSpan.Subtract(_period.StartTime).TotalMinutes) * _pixelPerMinute));
            else
            {
                throw new NotImplementedException();
            }
        }

        public TimeSpan GetTimeSpanFromPixel(int pixel)
        {
            throw new NotImplementedException();
        }
    }
}
