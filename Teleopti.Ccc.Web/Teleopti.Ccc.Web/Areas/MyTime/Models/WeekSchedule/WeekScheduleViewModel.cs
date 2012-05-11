using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule
{
	public class WeekScheduleViewModel
	{
		public PeriodSelectionViewModel PeriodSelection { get; set; }
		public IEnumerable<StyleClassViewModel> Styles { get; set; }
		public IEnumerable<DayViewModel> Days { get; set; }
		public bool TextRequestPermission { get; set; }
		public IEnumerable<AbsenceTypeViewModel> AbsenceTypes { get; set; }
	}

	public class AbsenceTypeViewModel
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
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
		public PeriodViewModel Summary { get; set; }
		public IEnumerable<PeriodViewModel> Periods { get; set; }

		public bool HasNote
		{
			get { return Note != null && !string.IsNullOrWhiteSpace(Note.Message); }
		}

	}

	public class NoteViewModel
	{
		public string Message { get; set; }
	}

	public class PeriodViewModel
	{
		public string Title { get; set; }
		public string TimeSpan { get; set; }
		public string Summary { get; set; }
		public string StyleClassName { get; set; }
		public MeetingViewModel Meeting { get; set; }
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

	public class MeetingViewModel
	{
		public string Title { get; set; }
		public string Location { get; set; }
	}

	public class HeaderViewModel
	{
		public string Title { get; set; }
		public string Date { get; set; }
		public string DayDescription { get; set; }
		public string DayNumber { get; set; }

	}
}
