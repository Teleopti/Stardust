using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceAndScheduleDayViewModel
	{
		public string Date { get; set; }
		public PreferenceDayViewModel Preference { get; set; }
		public DayOffDayViewModel DayOff { get; set; }
		public AbsenceDayViewModel Absence { get; set; }
		public PersonAssignmentDayViewModel PersonAssignment { get; set; }
		public bool Feedback { get; set; }
		public string StyleClassName { get; set; }
		public string BorderColor { get; set; }
		public IEnumerable<MeetingViewModel> Meetings { get; set; }
		public IEnumerable<PersonalShiftViewModel> PersonalShifts { get; set; }
		public BankHolidayCalendarViewModel BankHolidayCalendar { get; set; }
	}

	public class PersonalShiftViewModel
	{
		public string Subject { get; set; }
		public string TimeSpan { get; set; }
	}

	public class MeetingViewModel
	{
		public string Subject { get; set; }
		public string TimeSpan { get; set; }
		public bool IsOptional { get; set; }
	}
}