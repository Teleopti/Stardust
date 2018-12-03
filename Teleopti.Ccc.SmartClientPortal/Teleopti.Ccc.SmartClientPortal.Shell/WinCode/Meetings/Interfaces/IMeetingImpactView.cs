using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces
{
    public interface IMeetingImpactView : IMeetingDetailView
    {
		void SetStartDate(DateOnly startDate);
		void SetEndDate(DateOnly endDate);
		void SetStartTime(TimeSpan startTime);
		void SetEndTime(TimeSpan endTime);
    	void ShowWaiting();
    	void HideWaiting();
    	DateTime BestSlotSearchPeriodStart { get; }
		DateTime BestSlotSearchPeriodEnd { get; }
    	DateOnly StartDate { get; }
		TimeSpan StartTime { get; }
		TimeSpan EndTime { get; }
    	void SetSearchStartDate(DateOnly startDate);
		void SetSearchEndDate(DateOnly endDate);
    	void SetSearchInfo(string searchInfo);
    	void SetPreviousState(bool state);
    	void SetNextState(bool state);
        void RemoveAllSkillTabs();
		void AddSkillTab(string name, string description, int imageIndex, ISkill skill);
    	ISkill SelectedSkill();
		void DrawIntraday(ISkill skill, ISchedulerStateHolder schedulerStateHolder, IList<ISkillStaffPeriod> skillStaffPeriods);
    	void PositionControl();
    	void ClearTabPages();
		int IntervalsTotalWidth { get; }
		int ColsHeaderWidth { get; }
		int RowsHeight { get; }
		int RowHeaderHeight { get; }
    	TimeSpan IntervalStartValue();
		int GridColCount { get; }
		int ClientRectangleLeft { get; }
		int ClientRectangleRight { get; }
		int ClientRectangleTop { get; }
		object ResultGrid { get; }
    	void ShowMeeting(TransparentMeetingControlModel transparentMeetingControlModel, TransparentControlMeetingHelper transparentControlMeetingHelper);
		int GetCurrentHScrollPixelPos { get; }
    	void RefreshMeetingControl();
		bool IsRightToLeft { get; }
        bool FindButtonEnabled { get; set; }
        void ScrollMeetingIntoView(int pos);
    	bool HasStartInterval();
    	void SetSlotStartTime(TimeSpan timeSpan);
		void SetSlotEndTime(TimeSpan timeSpan);
    	void SetSlotEndDate(DateTime dateTime);
    }
}
