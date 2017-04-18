using System;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time
{
    /// <summary>
    /// For presenting a DateTime
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-09-03
    /// </remarks>
    public class DateTimeViewModel:DataModel
    {

        private DateTime _model;
        private bool _dateIsLocked;
        private bool _timeIsLocked;

        public DateTimeViewModel(DateTime dateTime)
        {
            _model = dateTime;
        }

        public DateTimeViewModel()
            : this(new DateTime())
        {

        }

        public DateTime DateTime
        {
            get { return _model; }
            set
            {
                if (value != _model)
                {
                    Date = value.Date;
                    Time = value.TimeOfDay;
                }
            }
        }

        public DateTime Date
        {
            get { return _model.Date; }
            set
            {
                if (value.Date != Date && !DateIsLocked)
                {
                    _model = value.Date.Add(Time);
                    SendPropertyChanged(nameof(Date));
                    SendPropertyChanged(nameof(DateTime));
                }
            }
        }

        public TimeSpan Time
        {
            get
            {
                return _model.TimeOfDay;
            }
            set
            {
                if (value != Time && !TimeIsLocked)
                {
                    _model = _model.Date.Add(value);
                    SendPropertyChanged(nameof(Time));
                    SendPropertyChanged(nameof(DateTime));
                }
            }
        }

        public bool DateIsLocked
        {
            get { return _dateIsLocked; }
            set
            {
                if(value!=DateIsLocked)
                {
                    _dateIsLocked = value;
                    SendPropertyChanged(nameof(DateIsLocked));

                }
            }
        }

        public bool TimeIsLocked
        {
            get { return _timeIsLocked; }
            set
            {
                if (value != TimeIsLocked)
                {
                    _timeIsLocked = value;
                    SendPropertyChanged(nameof(TimeIsLocked));
                }
            }
        }
    }
}
