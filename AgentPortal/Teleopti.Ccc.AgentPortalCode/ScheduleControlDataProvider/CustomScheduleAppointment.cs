#region Imports

using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Schedule;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

#endregion

namespace Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider
{

    /// <summary>
    /// Represent class that handls the functionality of item in shcedule control
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-11-18
    /// </remarks>
    public class CustomScheduleAppointment : ICustomScheduleAppointment
    {

        #region Fields - Instance Members

        private readonly IScheduleAppointment _scheduleAppointment;

        private Color _displayColor;

        private ScheduleAppointmentStatusTypes _status;

        private ScheduleAppointmentTypes _itemType;

        private bool _allowCopy;

        private bool _allowOpen;

        private bool _allowDelete;

        private bool _allowMultipleDaySplit;

        private bool _isSplittedPart;

        private ScheduleAppointmentPartType _splitPartType;

        #endregion

        #region Properties - Instance Members

        #region Properties - Instance Members - IScheduleAppointment Members

        public bool AllDay
        {
            get
            {
                return _scheduleAppointment.AllDay;
            }
            set
            {
                _scheduleAppointment.AllDay = value;
            }
        }

        public bool AllowClickable
        {
            get
            {
                return _scheduleAppointment.AllowClickable;
            }
            set
            {
                _scheduleAppointment.AllowClickable = value;
            }
        }

        public bool AllowDrag
        {
            get
            {
                return _scheduleAppointment.AllowDrag;
            }
            set
            {
                _scheduleAppointment.AllowDrag = value;
            }
        }

        public bool AllowResize
        {
            get
            {
                return _scheduleAppointment.AllowResize;
            }
            set
            {
                _scheduleAppointment.AllowResize = value;
            }
        }

        public Color BackColor
        {
            get
            {
                return _scheduleAppointment.BackColor;
            }
            set
            {
                _scheduleAppointment.BackColor = value;
            }
        }

        public string Content
        {
            get
            {
                return _scheduleAppointment.Content;
            }
            set
            {
                _scheduleAppointment.Content = value;
            }
        }

        public string CustomToolTip
        {
            get
            {
                return _scheduleAppointment.CustomToolTip;
            }
            set
            {
                _scheduleAppointment.CustomToolTip = value;
            }
        }

        public bool Dirty
        {
            get
            {
                return _scheduleAppointment.Dirty;
            }
            set
            {
                _scheduleAppointment.Dirty = value;
            }
        }

        public DateTime EndTime
        {
            get
            {
                return _scheduleAppointment.EndTime;
            }
            set
            {
                _scheduleAppointment.EndTime = value;
            }
        }

        public bool IgnoreChanges
        {
            get
            {
                return _scheduleAppointment.IgnoreChanges;
            }
            set
            {
                _scheduleAppointment.IgnoreChanges = value;
            }
        }

        public bool IsConflict(DateTime dtStart, DateTime dtEnd)
        {
            return _scheduleAppointment.IsConflict(dtStart, dtEnd);
        }

        public bool IsConflict(IScheduleAppointment item)
        {
            return _scheduleAppointment.IsConflict(item);
        }

        public int LabelValue
        {
            get
            {
                if (AgentScheduleStateHolder.Instance().ScheduleAppointmentColorTheme ==
                    ScheduleAppointmentColorTheme.DefaultColor)
                {
                    return 0; 
                }
                else
                {
                    return _scheduleAppointment.LabelValue;
                }
            }
            set
            {
                _scheduleAppointment.LabelValue = value;
            }
        }

        public string LocationValue
        {
            get
            {
                return _scheduleAppointment.LocationValue;
            }
            set
            {
                _scheduleAppointment.LocationValue = value;
            }
        }

        public int MarkerValue
        {
            get
            {
                return _scheduleAppointment.MarkerValue;
            }
            set
            {
                _scheduleAppointment.MarkerValue = value;
            }
        }

        public int Owner
        {
            get
            {
                return _scheduleAppointment.Owner;
            }
            set
            {
                _scheduleAppointment.Owner = value;
            }
        }

        public bool Reminder
        {
            get
            {
                return _scheduleAppointment.Reminder;
            }
            set
            {
                _scheduleAppointment.Reminder = value;
            }
        }

        public int ReminderValue
        {
            get
            {
                return _scheduleAppointment.ReminderValue;
            }
            set
            {
                _scheduleAppointment.ReminderValue = value;
            }
        }

        public DateTime StartTime
        {
            get
            {
                return _scheduleAppointment.StartTime;
            }
            set
            {
                _scheduleAppointment.StartTime = value;
            }
        }

        public string Subject
        {
            get
            {
                return _scheduleAppointment.Subject;
            }
            set
            {
                _scheduleAppointment.Subject = value;
            }
        }

        public object Tag
        {
            get
            {
                return _scheduleAppointment.Tag;
            }
            set
            {
                _scheduleAppointment.Tag = value;
            }
        }

        public Color TimeSpanColor
        {
            get
            {
                return _scheduleAppointment.TimeSpanColor;
            }
            set
            {
                _scheduleAppointment.TimeSpanColor = value;
            }
        }

        public ScheduleAppointmentToolTip ToolTip
        {
            get
            {
                return _scheduleAppointment.ToolTip;
            }
            set
            {
                _scheduleAppointment.ToolTip = value;
            }
        }

        public int UniqueID
        {
            get
            {
                return _scheduleAppointment.UniqueID;
            }
            set
            {
                _scheduleAppointment.UniqueID = value;
            }
        }

        public int Version
        {
            get { return _scheduleAppointment.Version; }
        }

        //public string DisplayItem(string timeFormat, CultureInfo culture)
        //{
        //    string displayItem = string.Format(CultureInfo.CurrentCulture, "{0} ", Subject);

        //    if (_itemType != ScheduleAppointmentTypes.DayOff) //Dayoff should'nt show time
        //    {
        //        displayItem += StartTime.ToString(timeFormat, culture);
        //        displayItem += " - " + EndTime.ToString(timeFormat, culture);
        //    }
        //    return displayItem;
        //}

        #endregion

        #region Properties - Instance Members - ICustomScheduleAppointment Members

        public Color DisplayColor
        {
            get
            {
                return _displayColor;
            }
            set
            {
                _displayColor = value;
            }
        }

        public ScheduleAppointmentStatusTypes Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
            }
        }

        public ScheduleAppointmentTypes AppointmentType
        {
            get { return _itemType; }
            set { _itemType = value; }
        }

        public bool IsSplit
        {
            get { return _isSplittedPart; }
            set { _isSplittedPart = value; }
        }

        public ScheduleAppointmentPartType SplitPartType
        {
            get { return _splitPartType; }
            set { _splitPartType = value; }
        }

        public bool AllowCopy
        {
            get
            {
                return _allowCopy;
            }
            set
            {
                _allowCopy = value;
            }
        }

        public bool AllowOpen
        {
            get
            {
                return _allowOpen;
            }
            set
            {
                _allowOpen = value;
            }
        }

        public bool AllowDelete
        {
            get
            {
                return _allowDelete;
            }
            set
            {
                _allowDelete = value;
            }
        }

        public bool AllowMultipleDaySplit
        {
            get
            {
                return _allowMultipleDaySplit;
            }
            set
            {
                _allowMultipleDaySplit = value;
            }
        }

        

        #endregion

        #endregion

        #region Methods - Instance Members

        #region Methods - Instance Members - CustomScheduleAppointment Members

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return _scheduleAppointment.CompareTo(obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(IScheduleAppointment other)
        {
            bool areEqual = false;
            Dto sourceDto = _scheduleAppointment.Tag as Dto;
            Dto foriegnDto = other.Tag as Dto;

            if ((sourceDto != null) && (foriegnDto != null))
            {
                if (foriegnDto.Id == null)
                {
                    areEqual = sourceDto == foriegnDto;
                }
                else
                {
                    areEqual = sourceDto.Id == foriegnDto.Id;
                }
            }

            return areEqual;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if obj and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            ICustomScheduleAppointment app = obj as ICustomScheduleAppointment;
            if (app == null)
            {
                return false;
            }
            else
            {
                return Equals(app);
            }
        }

        /// <summary>
        /// Operator ==.
        /// </summary>
        /// <param name="wt1">The work time 1.</param>
        /// <param name="wt2">The work time 2.</param>
        /// <returns></returns>
        public static bool operator ==(CustomScheduleAppointment app1, CustomScheduleAppointment app2)
        {
            return app1.Equals(app2);
        }

        /// <summary>
        /// Operator !=.
        /// </summary>
        /// <param name="wt1">The work time 1.</param>
        /// <param name="wt2">The work time 2.</param>
        /// <returns></returns>
        public static bool operator !=(CustomScheduleAppointment app1, CustomScheduleAppointment app2)
        {
            return !app2.Equals(app1);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return _scheduleAppointment.GetHashCode();
        }

        public static bool operator >(CustomScheduleAppointment app1, CustomScheduleAppointment app2)
        {
            return app1.StartTime > app2.StartTime;
        }

        public static bool operator <(CustomScheduleAppointment app1, CustomScheduleAppointment app2)
        {
            return app1.StartTime < app2.StartTime;
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            IScheduleAppointment clonedScheduleAppointment = (IScheduleAppointment)_scheduleAppointment.Clone();
            ICustomScheduleAppointment newCustomScheduleappointment =
                new CustomScheduleAppointment(clonedScheduleAppointment);

            newCustomScheduleappointment.AppointmentType = _itemType;
            newCustomScheduleappointment.Status = _status;
            newCustomScheduleappointment.DisplayColor = _displayColor;
            newCustomScheduleappointment.AllowCopy = _allowCopy;
            newCustomScheduleappointment.AllowDelete = _allowDelete;
            newCustomScheduleappointment.AllowOpen = _allowOpen;
            newCustomScheduleappointment.AllowMultipleDaySplit = _allowMultipleDaySplit;
            newCustomScheduleappointment.IsSplit = _isSplittedPart;
            newCustomScheduleappointment.SplitPartType = _splitPartType;
            newCustomScheduleappointment.Subject = _scheduleAppointment.Subject;
            return newCustomScheduleappointment;
        }

        #endregion

        #endregion

        #region Methods - Instance Members - CustomScheduleAppointment Members - (constructors)

        public CustomScheduleAppointment()
        {
            _scheduleAppointment = new ScheduleAppointment();
        }

        //as a wrapper
        public CustomScheduleAppointment(IScheduleAppointment scheduleAppointment)
        {
            _scheduleAppointment = scheduleAppointment;
        }

        //Have to override ToString(), in some strange cases SF ScheduleControl
        //shows the classname instead of the Subject?!?
        public override string ToString()
        {
            return _scheduleAppointment.Subject;
        }
        #endregion

        #endregion

    }
}
