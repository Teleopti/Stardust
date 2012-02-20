using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceViewModel
	{
		public PeriodSelectionViewModel PeriodSelection { get; set; }
		public IEnumerable<WeekDayHeader> WeekDayHeaders { get; set; }
		public IEnumerable<WeekViewModel> Weeks { get; set; }
		public PreferencePeriodViewModel PreferencePeriod { get; set; }
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
		public IEnumerable<DayViewModelBase> Days { get; set; }
	}

	public class DayViewModelBase
	{
		public DateOnly Date { get; set; }
		public DayState State { get; set; }
		public HeaderViewModel Header { get; set; }
		public string StyleClassName { get; set; }
	}

	[Flags]
	public enum DayState
	{
		None = 0,
		Editable = 1
	}

	public class HeaderViewModel
	{
		public string DayDescription { get; set; }
		public string DayNumber { get; set; }
	}

	public class PreferenceDayViewModel : DayViewModelBase
	{
		public string Preference { get; set; }
		public string PossibleStartTimes { get; set; }
		public string PossibleEndTimes { get; set; }
		public string PossibleContractTimes { get; set; }
	}

	public class ScheduledDayViewModel : DayViewModelBase
	{
		public string ShiftCategory { get; set; }
		public string TimeSpan { get; set; }
		public string ContractTime { get; set; }
	}
}