using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces
{
	public interface IMeetingSchedulesView : IMeetingDetailView
	{
		void SetStartDate(DateOnly startDate);
		void SetEndDate(DateOnly endDate);
		void SetStartTime(TimeSpan startTime);
		void SetEndTime(TimeSpan endTime);
		void SetCurrentDate(DateOnly currentDate);
		void RefreshGrid();
		void SetRecurringDates(IList<DateOnly> recurringDates);
		TimeSpan SetSuggestListStartTime { get; set; }
		TimeSpan SetSuggestListEndTime { get; set; }
		void NotifyMeetingDatesChanged();
		bool IsRightToLeft { get; }
		bool TimeFocused { get; set; }
		void SetSizeWECursor();
		void SetDefaultCursor();
		void SetHandCursor();
		void RefreshGridSchedules();
		void ScrollMeetingIntoView();
		void UnBindEvents();
		void BindEvents();
	}
}
