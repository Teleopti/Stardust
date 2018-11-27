﻿using System;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces
{
    public interface IMeetingRecurrenceView : IMeetingDetailView, IViewBase
    {
        void Close();
        void SetEndTime(TimeSpan endTime);
        void SetStartTime(TimeSpan startTime);
        void SetStartDate(DateOnly startDate);
        void SetRecurringEndDate(DateOnly endDate);
        void SetRecurringOption(RecurrentMeetingType recurrentMeetingType);
        void RefreshRecurrenceOption(RecurrentMeetingOptionViewModel meetingOptionViewModel);
        void SetRecurringExists(bool recurringExists);
        void AcceptAndClose();
    	void NotifyMeetingTimeChanged();
    }
}
