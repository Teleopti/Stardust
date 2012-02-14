using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Common.Time
{
    public class TimeDurationPickerPresenter
    {
        private readonly ITimeDurationPickerView _view;
        private TimeSpan _interval;

        public TimeDurationPickerPresenter(ITimeDurationPickerView view)
        {
            _view = view;
            Interval = new TimeSpan(0, 30, 0);
        }

        public IList<TimeSpanDataBoundItem> CreateTimeList(TimeSpan from, TimeSpan to)
        {
            IList<TimeSpanDataBoundItem> timeList = new List<TimeSpanDataBoundItem>();
            DateTime maxTime = DateTime.MinValue.Add(to.Add(new TimeSpan(1)));
            for (DateTime timeOfDay = DateTime.MinValue.Add(from);
                timeOfDay < maxTime;
                timeOfDay = timeOfDay.Add(_interval))
            {
                timeList.Add(new TimeSpanDataBoundItem(timeOfDay.TimeOfDay));
            }
            return timeList;
        }


        public TimeSpan Interval
        {
            get { return _interval; }
            set
            {
                _interval = value;
                IList<TimeSpanDataBoundItem> timeSpans = CreateTimeList(TimeSpan.Zero, new TimeSpan(23, 59, 59));
                _view.SetTimeList(timeSpans);
            }
        }
    }
}