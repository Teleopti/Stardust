using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview
{
    public class AppointmentFromMeetingCreator
    {
        private readonly MeetingTimeZoneHelper _meetingTimeZoneHelper;
        
        public AppointmentFromMeetingCreator(MeetingTimeZoneHelper meetingTimeZoneHelper)
        {
            _meetingTimeZoneHelper = meetingTimeZoneHelper;
        }

        public IList<ISimpleAppointment> GetAppointments(ICollection<IMeeting> meetings, DateOnly startDate, DateOnly endDate)
        {
            var retList = new List<ISimpleAppointment>();
            if(meetings != null)
            {
                foreach (var meeting in meetings)
                {
                    retList.AddRange(GetAppointments(meeting, startDate, endDate));
                }
            }
            
            return retList;
        }

        public IList<ISimpleAppointment> GetAppointments(IMeeting meeting, DateOnly startDate, DateOnly endDate)
        {
            var retList = new List<ISimpleAppointment>();
            if(meeting != null)
            {
                var meetingTimeZone = meeting.TimeZone;
            
                var dates = meeting.GetRecurringDates();
                foreach (var dateOnly in dates)
                {
                    if(dateOnly < startDate || dateOnly > endDate)
                        continue;
                    var localStartTime = dateOnly.Date.Add(meeting.StartTime);
                    var appointment = getAppointment(meeting, localStartTime, meetingTimeZone);
                    retList.Add(appointment);
                    retList.AddRange(splitAppointment(appointment));
                }
            }
            
            return retList;
        }

        private static IEnumerable<ISimpleAppointment> splitAppointment(ISimpleAppointment appointment)
        {
            var appointments = new List<ISimpleAppointment>();
            var endDateTime = appointment.EndDateTime;

            if (appointment.StartDateTime.Date.Equals(endDateTime.Date))
                return appointments;

            if (appointment.EndDateTime.TimeOfDay == TimeSpan.FromHours(0))
                return appointments;

            var newStart = appointment.StartDateTime.Date.AddDays(1);

            var newappointment = new SimpleAppointment
                                     {
                                         Meeting = appointment.Meeting,
                                         StartDateTime = newStart,
                                         PreviousAppointment = appointment,
                                         EndDateTime = appointment.EndDateTime
            };
            
            appointment.NextAppointment = newappointment;
            appointments.Add(newappointment);
            appointments.AddRange(splitAppointment(newappointment));
            return appointments;
        }

        private ISimpleAppointment getAppointment(IMeeting meeting, DateTime localStartTime, TimeZoneInfo meetingTimeZone)
        {
            var duration = meeting.MeetingDuration();
            var app = new SimpleAppointment
                       {
                           Meeting = meeting,
                           StartDateTime = _meetingTimeZoneHelper.ConvertToUserTimeZone(localStartTime, meetingTimeZone)
                       };

            var localEndTime = app.StartDateTime.Add(duration);

            app.EndDateTime = MeetingTimeZoneHelper.CheckSoValidInTimeZone(localEndTime, _meetingTimeZoneHelper.UserTimeZone);
            return app;
        }
    }

    public interface ISimpleAppointment
    {
        DateTime StartDateTime { get; set; }
        DateTime EndDateTime { get; set; }
        //TimeSpan EndTime { get; set; }
        IMeeting Meeting { get; set; }
        ISimpleAppointment PreviousAppointment { get; set; }
        ISimpleAppointment NextAppointment { get; set; }
        ISimpleAppointment FirstAppointment { get; }
        ISimpleAppointment LastAppointment { get; }
    	bool OtherHasBeenDeleted { get; set; }
    }

    public class SimpleAppointment : ISimpleAppointment
    {
        private ISimpleAppointment _previousAppointment;

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        //public TimeSpan EndTime { get; set; }
        public IMeeting Meeting { get; set; }
        
        public ISimpleAppointment PreviousAppointment
        {
            get { return _previousAppointment; }
            set
            {
                _previousAppointment = value;
                if(_previousAppointment != null)
                    _previousAppointment.NextAppointment = this;
            }
        }

        public ISimpleAppointment NextAppointment { get; set; }

        public ISimpleAppointment FirstAppointment
        {
            get
            {
                if (PreviousAppointment == null)
                    return this;
                return PreviousAppointment.FirstAppointment;
            }
        }

        public ISimpleAppointment LastAppointment
        {
            get
            {
                if (NextAppointment == null)
                    return this;
                return NextAppointment.LastAppointment;
            }
        }

    	public bool OtherHasBeenDeleted { get; set; }
    	
    }
}