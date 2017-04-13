using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls
{
    public class IntervalLengthItem
    {
        private readonly int _totalMinutes;

        public IntervalLengthItem(int totalMinutes)
        {
            _totalMinutes = totalMinutes;
        }

        public string Text
        {
            get
            {
                var time = TimeSpan.FromMinutes(_totalMinutes);
                return TimeHelper.GetLongHourMinuteTimeString(time, CultureInfo.CurrentCulture);
            }
        }

        public int Minutes
        {
            get
            {
                return _totalMinutes;
            }
        }
    }
}
