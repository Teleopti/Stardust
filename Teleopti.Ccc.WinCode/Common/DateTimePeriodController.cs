using System;
using System.Collections.Generic;
using System.ComponentModel;
using InParameter=Teleopti.Interfaces.Domain.InParameter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Controller for handling DateTimePeriod from both wpf and winform.
    /// It will always hold a valid DateTimePeriod with a minimum diff of its own interval
    /// It will also provide a collection of DateTimes (diff of interval) for start and end controls
    /// Implements NotifyPropertyChanged for databinding
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-07-10
    /// </remarks>
    public class DateTimePeriodController:INotifyPropertyChanged
    {
        #region fields
        private Interfaces.Domain.DateTimePeriod _dateTimePeriod;
        private TimeSpan _interval;
        private TimeSpan _startInterval;
        private TimeSpan _endInterval;

        #endregion //fields

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePeriodController"/> class.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="interval">The interval.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-10
        /// </remarks>
        public DateTimePeriodController(Interfaces.Domain.DateTimePeriod dateTimePeriod,TimeSpan interval):this(dateTimePeriod,interval,TimeSpan.FromHours(12),TimeSpan.FromHours(12)){}


        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePeriodController"/> class.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="startInterval">The start interval.</param>
        /// <param name="endInterval">The end interval.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-14
        /// </remarks>
        public DateTimePeriodController(Interfaces.Domain.DateTimePeriod dateTimePeriod,TimeSpan interval,TimeSpan startInterval,TimeSpan endInterval)
        {
            _dateTimePeriod = dateTimePeriod;
            _interval = interval;
            _startInterval = startInterval;
            _endInterval = endInterval;
            if (StartDateTime.Add(Interval) > EndDateTime) 
                StartDateTime = DateTimePeriod.StartDateTime;
        }

        #endregion //ctor

        #region public

        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>The interval.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-10
        /// </remarks>
        public TimeSpan Interval
        {
            get { return _interval; }
            set
            {
                InParameter.ValueMustBePositive("Interval",((TimeSpan)value).Minutes);
                _interval = value;
                NotifyPropertyChanged("Interval");
                if (StartDateTime.Add(Interval) > EndDateTime)
                {
                    DateTimePeriod = new DateTimePeriod(DateTimePeriod.StartDateTime, StartDateTime.Add(Interval));
                    NotifyPropertyChanged("EndDateTime");
                }
            }
        }
        /// <summary>
        /// Gets the date time period.
        /// </summary>
        /// <value>The date time period.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-10
        /// </remarks>
        public DateTimePeriod DateTimePeriod
        {
            get { return _dateTimePeriod; }
            private set 
            { 
                _dateTimePeriod = value;
                NotifyPropertyChanged("DateTimePeriod");
            }
        }

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>The start date time.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-10
        /// </remarks>
        public DateTime StartDateTime
        {
            get { return _dateTimePeriod.StartDateTime; }
            set
            {
                if (value.Add(_interval) <= _dateTimePeriod.EndDateTime)
                {
                    DateTimePeriod = new DateTimePeriod(value, DateTimePeriod.EndDateTime);
                }
                else
                {
                    DateTimePeriod = new DateTimePeriod(value, value.Add(_interval));
                    NotifyPropertyChanged("EndDateTime");
                }

                NotifyPropertyChanged("StartDateTime");
            }
        }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>The end date time.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-10
        /// </remarks>
        public DateTime EndDateTime
        {
            get { return _dateTimePeriod.EndDateTime; }
            set 
            {
                if (value.Subtract(Interval) >= DateTimePeriod.StartDateTime)
                {
                    DateTimePeriod = new DateTimePeriod(DateTimePeriod.StartDateTime,value);
                }
                else
                {
                    DateTimePeriod = new DateTimePeriod(value.Subtract(_interval), value);
                    NotifyPropertyChanged("StartDateTime");
                }
                NotifyPropertyChanged("EndDateTime");
            }
        }

        /// <summary>
        /// Gets or sets the StartInterval.
        /// Determines how long the period to pick StartDateTime from should be
        /// </summary>
        /// <value>The start interval.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-14
        /// </remarks>
        public TimeSpan StartInterval
        {
            get { return _startInterval; }
            set
            {
                _startInterval = value;
              
            }
        }

        /// <summary>
        /// Gets or sets the EndInterval.
        /// Determines how long the period to pick EndDateTime from should be
        /// </summary>
        /// <value>The end interval.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-14
        /// </remarks>
        public TimeSpan EndInterval
        {
            get { return _endInterval; }
            set
            {
                _endInterval = value;
               
            }
        }


        /// <summary>
        /// Returns a collection of DateTimes that can be used as StartDateTimes
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-14
        /// </remarks>
        public IList<DateTime> StartTimeSelection
        {
            get
            {
                IList<DateTime> retList = new List<DateTime>();
                DateTime toAdd = StartDateTime;
                while (StartDateTime.Subtract(StartInterval) <= toAdd)
                {
                    retList.Add(toAdd);
                    toAdd = toAdd.Subtract(Interval);
                }
                return retList;
            }
        }

        /// <summary>
        /// Returns a collection of DateTimes that can be used as EndDateTimes
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-14
        /// </remarks>
        public IList<DateTime> EndTimeSelection
        {
            get
            {
                IList<DateTime> retList = new List<DateTime>();
                DateTime toAdd = EndDateTime;
                while (EndDateTime.Add(EndInterval) >= toAdd)
                {
                    retList.Add(toAdd);
                    toAdd = toAdd.Add(Interval);
                }
                return retList;
            }
        }

        #endregion //public

        #region private methods
        

        #endregion //private methods

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-11
        /// </remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
        	var handler = PropertyChanged;
            if (handler!=null)
            {
                handler.Invoke(this,new PropertyChangedEventArgs(property));
            }
        }
        #endregion //INotifyPropertyChanged Members

        
    }
}
