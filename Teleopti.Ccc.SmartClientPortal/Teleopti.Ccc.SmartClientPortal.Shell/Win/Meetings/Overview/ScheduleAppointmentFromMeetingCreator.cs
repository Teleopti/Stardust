using System.Collections.Generic;
using Syncfusion.Schedule;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Meetings.Overview;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings.Overview
{
    public interface IScheduleAppointmentFromMeetingCreator
    {
        ScheduleAppointmentList GetAppointmentList(IEnumerable<ISimpleAppointment> simpleAppointments, ListObjectList labelList);
    }

    public class ScheduleAppointmentFromMeetingCreator : IScheduleAppointmentFromMeetingCreator
    {
        public ScheduleAppointmentList GetAppointmentList(IEnumerable<ISimpleAppointment> simpleAppointments, ListObjectList labelList)
        {
            var list = new ScheduleAppointmentList();
            if(simpleAppointments != null)
            {
                foreach (var simpleAppointment in simpleAppointments)
                {
                    var meeting = simpleAppointment.Meeting;
                	var appointment = new TeleoptiScheduleAppointment
                	                  	{
                	                  		SimpleAppointment = simpleAppointment,
                	                  		StartTime = simpleAppointment.StartDateTime,
                	                  		EndTime = simpleAppointment.EndDateTime,
                	                  		Subject = meeting.GetSubject(new NoFormatting()),
                	                  		LocationValue = meeting.GetLocation(new NoFormatting()),
                	                  		LabelValue = getLabelValue(meeting.Activity, labelList),
                	                  		Content = meeting.GetDescription(new NoFormatting()),
                	                  		Tag = meeting,
                	                  		MarkerValue = simpleAppointment.OtherHasBeenDeleted ? 1 : 0,
                	                  		AllDay = false
                    };
					
                    list.Add(appointment);
                }
            }
            
            return list;
        }

        private static int getLabelValue(IActivity activity, ListObjectList labelList)
        {
            foreach (ActivityListObject activityListObject in labelList)
            {
                if (activityListObject.Activity.Equals(activity))
                    return activityListObject.ValueMember;
            }

            return -1;
        }
    }

    public class TeleoptiScheduleAppointment : ScheduleAppointment
    {
        public ISimpleAppointment SimpleAppointment { get; set; }
    }
}