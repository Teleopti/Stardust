using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;


namespace Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability
{
	public class StudentAvailabilityViewModel
	{
		public PeriodSelectionViewModel PeriodSelection { get; set; }
		public IEnumerable<StyleClassViewModel> Styles { get; set; }

		public IEnumerable<WeekDayHeader> WeekDayHeaders { get; set; }
		public IEnumerable<WeekViewModel> Weeks { get; set; }

		public PeriodSummaryViewModel PeriodSummary { get; set; }

		public StudentAvailabilityPeriodViewModel StudentAvailabilityPeriod { get; set; }
	}

	public class StudentAvailabilityPeriodViewModel
	{
		public string Period { get; set; }
		public string OpenPeriod { get; set; }
	}

	public class PeriodSummaryViewModel
	{
		public bool IsWorkTimeInRange { get; set; }
		public string Message { get; set; }
		public string Summary { get; set; }
	}

	public class WeekViewModel
	{
		public WeekSummaryViewModel Summary { get; set; }
		public IEnumerable<DayViewModelBase> Days { get; set; }
	}

	public class WeekSummaryViewModel
	{
		public string MinWorkTime { get; set; }
		public string MaxWorkTime { get; set; }
	}

	public class DayViewModelBase
	{
		public DateOnly Date { get; set; }
		public HeaderViewModel Header { get; set; }
		public bool InPeriod { get; set; }
		public bool Editable { get; set; }
	}

	public class AvailableDayViewModel : DayViewModelBase
	{
	}

	public class HeaderViewModel
	{
		public string DayDescription { get; set; }
		public string DayNumber { get; set; }
	}

	public class WeekDayHeader
	{
		public string Title { get; set; }
	}
}