using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule
{
	public class BaseScheduleViewModel
	{
		public RequestPermission RequestPermission { get; set; }
		public IEnumerable<TimeLineViewModel> TimeLine { get; set; }
		public bool AsmPermission { get; set; }
		public bool ViewPossibilityPermission { get; set; }
		public string DatePickerFormat { get; set; }
		public DaylightSavingsTimeAdjustmentViewModel DaylightSavingTimeAdjustment { get; set; }
		public double BaseUtcOffsetInMinutes { get; set; }
		public bool CheckStaffingByIntraday { get; set; }
	}

	public class DayScheduleViewModel: BaseScheduleViewModel
	{
		public int UnReadMessageCount { get; set; }
		public string Date { get; set; }
		public bool IsToday { get; set; }
		public DayViewModel Schedule { get; set; }
	    public ShiftTradeRequestsPeriodViewModel ShiftTradeRequestSetting { get; set; }
	}

	public class WeekScheduleViewModel : BaseScheduleViewModel
	{
		public PeriodSelectionViewModel PeriodSelection { get; set; }
		public IEnumerable<DayViewModel> Days { get; set; }
		public bool IsCurrentWeek { get; set; }
		public string CurrentWeekEndDate { get; set; }
		public string CurrentWeekStartDate { get; set; }
		public IEnumerable<StyleClassViewModel> Styles { get; set; }
	}

	public class RequestPermission
	{
		public bool TextRequestPermission { get; set; }
		public bool AbsenceRequestPermission { get; set; }
		public bool ShiftTradeRequestPermission { get; set; }
		public bool OvertimeAvailabilityPermission { get; set; }
		public bool AbsenceReportPermission { get; set; }
		public bool ShiftExchangePermission { get; set; }
		public bool ShiftTradeBulletinBoardPermission { get; set; }
		public bool PersonAccountPermission { get; set; }
	}

	public class AbsenceTypeViewModel
	{
		public string Name { get; set; }
		public Guid? Id { get; set; }
	}

	public class TimeLineViewModel
	{
		public TimeSpan Time { get; set; }
		public string TimeLineDisplay { get; set; }
		public decimal PositionPercentage { get; set; }
		public string TimeFixedFormat { get; set; }
	}

	[Flags]
	public enum SpecialDateState
	{
		None = 0,
		Selected = 1,
		Today = 2
	}

	public class DayViewModel
	{
		public int TextRequestCount { get; set; }
		public string Date { get; set; }
		public string FixedDate { get; set; }
		public SpecialDateState State { get; set; }
		public HeaderViewModel Header { get; set; }
		public NoteViewModel Note { get; set; }
		public OvertimeAvailabilityViewModel OvertimeAvailabililty { get; set; }
		public bool HasMainShift { get; set; }
		public bool HasOvertime { get; set; }
		public bool IsFullDayAbsence { get; set; }
		public bool IsDayOff { get; set; }
		public PeriodViewModel Summary { get; set; }
		public IEnumerable<PeriodViewModel> Periods { get; set; }
		public int DayOfWeekNumber { get; set; }
		public bool Availability { get; set; }
		public bool HasNote => !string.IsNullOrWhiteSpace(Note?.Message);
		public string ProbabilityClass { get; set; }
		public string ProbabilityText { get; set; }
		public IList<OccupancyViewModel> SeatBookings { get; set; }
		public TimePeriod? OpenHourPeriod { get; set; }
		public bool HasNotScheduled { get; set; }
	}

	public class OvertimeAvailabilityViewModel
	{
		public bool HasOvertimeAvailability { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public bool EndTimeNextDay { get; set; }
		public string DefaultStartTime { get; set; }
		public string DefaultEndTime { get; set; }
		public bool DefaultEndTimeNextDay { get; set; }
	}

	public class NoteViewModel
	{
		public string Message { get; set; }
	}

	public class PeriodViewModel
	{
		public string Title { get; set; }
		public string TimeSpan { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Summary { get; set; }
		public string StyleClassName { get; set; }
		public MeetingViewModel Meeting { get; set; }
		public decimal StartPositionPercentage { get; set; }
		public decimal EndPositionPercentage { get; set; }
		public string Color { get; set; }
		public bool IsOvertime { get; set; }
	}

	public class PersonDayOffPeriodViewModel : PeriodViewModel
	{
	}

	public class PersonAssignmentPeriodViewModel : PeriodViewModel
	{
	}

	public class FullDayAbsencePeriodViewModel : PeriodViewModel
	{
	}

	public class OvertimeAvailabilityPeriodViewModel : PeriodViewModel
	{
		public bool IsOvertimeAvailability { get; set; }
		public OvertimeAvailabilityViewModel OvertimeAvailabilityYesterday { get; set; }
	}

	public class MeetingViewModel
	{
		public string Title { get; set; }
		public string Location { get; set; }
		public string Description { get; set; }
	}

	public class HeaderViewModel
	{
		public string Title { get; set; }
		public string Date { get; set; }
		public string DayDescription { get; set; }
		public string DayNumber { get; set; }
	}

	public class DaylightSavingsTimeAdjustmentViewModel
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double AdjustmentOffsetInMinutes { get; set; }

		public DaylightSavingsTimeAdjustmentViewModel(DaylightTime daylightTime)
		{
			StartDateTime = daylightTime.Start;
			EndDateTime = daylightTime.End;
			AdjustmentOffsetInMinutes = daylightTime.Delta.TotalMinutes;
		}
	}
}