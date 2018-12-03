using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;


namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common
{
	public class DayViewModel
	{
		public int RequestsCount { get; set; }
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
}