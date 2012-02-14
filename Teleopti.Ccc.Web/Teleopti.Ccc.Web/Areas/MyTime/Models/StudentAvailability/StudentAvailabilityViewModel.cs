using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Interfaces.Domain;

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

	public class StyleClassViewModel
	{
		public string Name { get; set; }
		public string ColorHex { get; set; }
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
		public DayState State { get; set; }
		public HeaderViewModel Header { get; set; }
		public string StyleClassName { get; set; }
	}

	[Flags]
	public enum DayState
	{
		None = 0,
		Editable = 1,
		Deletable = 2
	}

	public class AvailableDayViewModel : DayViewModelBase
	{
		public string AvailableTimeSpan { get; set; }
		public string StartTimeSpan { get; set; }
		public string EndTimeSpan { get; set; }
		public string WorkTimeSpan { get; set; }
	}

	public class ScheduledDayViewModel : DayViewModelBase
	{
		public string Title { get; set; }
		public string TimeSpan { get; set; }
		public string Summary { get; set; }
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