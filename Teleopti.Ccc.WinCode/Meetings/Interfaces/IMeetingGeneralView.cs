using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Interfaces
{
    public interface IMeetingGeneralView : IMeetingDetailView
    {
        void SetOrganizer(string organizer);
        void SetParticipants(string participants);
        void SetActivityList(IList<IActivity> activities);
        void SetSelectedActivity(IActivity activity);
        void SetTimeZoneList(IList<ICccTimeZoneInfo> timeZoneList);
        void SetSelectedTimeZone(ICccTimeZoneInfo timeZone);
        void SetStartDate(DateOnly startDate);
        void SetEndDate(DateOnly endDate);
        void SetStartTime(TimeSpan startTime);
        void SetEndTime(TimeSpan endTime);
        void SetRecurringEndDate(DateOnly recurringEndDate);
        void SetSubject(string subject);
        void SetLocation(string location);
        void SetDescription(string description);
    	void NotifyMeetingTimeChanged();
    }
}
