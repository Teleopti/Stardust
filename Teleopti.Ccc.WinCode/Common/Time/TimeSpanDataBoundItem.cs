using System;

namespace Teleopti.Ccc.WinCode.Common.Time
{
    public class TimeSpanDataBoundItem
    {
        private TimeSpan _timeSpan;

        public TimeSpanDataBoundItem(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
        }

        public TimeSpan TimeSpan
        {
            get { return _timeSpan; }
        }

        public string FormattedText
        {
            get
            {
                string minutes = ("0" + _timeSpan.Minutes);
                minutes = minutes.Substring(minutes.Length - 2, 2);
                string timeText = _timeSpan.Hours + ":" + minutes;
                return timeText;
            }
        }
    }
}