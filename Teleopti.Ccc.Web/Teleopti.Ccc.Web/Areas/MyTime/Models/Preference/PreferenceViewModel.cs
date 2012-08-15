using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceViewModel
	{
		public PeriodSelectionViewModel PeriodSelection { get; set; }
		public IEnumerable<WeekDayHeader> WeekDayHeaders { get; set; }
		public IEnumerable<WeekViewModel> Weeks { get; set; }
		public PreferencePeriodViewModel PreferencePeriod { get; set; }
		public IEnumerable<StyleClassViewModel> Styles { get; set; }
	}

	public class PreferencePeriodViewModel
	{
		public string Period { get; set; }
		public string OpenPeriod { get; set; }
	}

	public class WeekDayHeader
	{
		public string Title { get; set; }
	}

	public class WeekViewModel
	{
		public IEnumerable<DayViewModel> Days { get; set; }
	}

	public class DayViewModel
	{
		public DateOnly Date { get; set; }
		public bool InPeriod { get; set; }
		public bool Editable { get; set; }
		public bool Feedback { get; set; }
		public HeaderViewModel Header { get; set; }
		public string StyleClassName { get; set; }
		public string BorderColor { get; set; }
		public PreferenceDayViewModel Preference { get; set; }
		public PersonAssignmentDayViewModel PersonAssignment { get; set; }
		public DayOffDayViewModel DayOff { get; set; }
		public AbsenceDayViewModel Absence { get; set; }
	}

	public class HeaderViewModel
	{
		public string DayDescription { get; set; }
		public string DayNumber { get; set; }
	}

	public class PreferenceDayViewModel
	{
		public string Preference { get; set; }
		public bool Extended { get; set; }
	}

	public class PersonAssignmentDayViewModel
	{
		public string ShiftCategory { get; set; }
		public string TimeSpan { get; set; }
		public string ContractTime { get; set; }
		public int ContractTimeMinutes { get; set; }
	}

	public class DayOffDayViewModel
	{
		public string DayOff { get; set; }
	}

	public class AbsenceDayViewModel
	{
		public string Absence { get; set; }
	}

}