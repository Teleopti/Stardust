using System;
using System.Drawing;
using Syncfusion.Schedule;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

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
        private readonly IScheduleAppointment _scheduleAppointment;

        private Color _displayColor;

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

	    public bool RecurringOnOverride
	    {
		    get { return _scheduleAppointment.RecurringOnOverride; }
		    set { _scheduleAppointment.RecurringOnOverride = value; }
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

        public ScheduleAppointmentStatusTypes Status { get; set; }

        public ScheduleAppointmentTypes AppointmentType { get; set; }

        public bool IsSplit { get; set; }

        public ScheduleAppointmentPartType SplitPartType { get; set; }

        public bool AllowCopy { get; set; }

        public bool AllowOpen { get; set; }

        public bool AllowDelete { get; set; }

        public bool AllowMultipleDaySplit { get; set; }

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

        public object Clone()
        {
            IScheduleAppointment clonedScheduleAppointment = (IScheduleAppointment)_scheduleAppointment.Clone();
            ICustomScheduleAppointment newCustomScheduleappointment =
                new CustomScheduleAppointment(clonedScheduleAppointment);

            newCustomScheduleappointment.AppointmentType = AppointmentType;
            newCustomScheduleappointment.Status = Status;
            newCustomScheduleappointment.DisplayColor = _displayColor;
            newCustomScheduleappointment.AllowCopy = AllowCopy;
            newCustomScheduleappointment.AllowDelete = AllowDelete;
            newCustomScheduleappointment.AllowOpen = AllowOpen;
            newCustomScheduleappointment.AllowMultipleDaySplit = AllowMultipleDaySplit;
            newCustomScheduleappointment.IsSplit = IsSplit;
            newCustomScheduleappointment.SplitPartType = SplitPartType;
            newCustomScheduleappointment.Subject = _scheduleAppointment.Subject;
            return newCustomScheduleappointment;
        }
    }
}
