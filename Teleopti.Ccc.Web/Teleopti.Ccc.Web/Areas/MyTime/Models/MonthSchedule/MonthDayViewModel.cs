using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule
{
	public class MonthDayViewModel
	{
		public DateTime Date { get; set; }
		public string FixedDate { get; set; }
		public AbsenceViewModel[] Absences { get; set; }
		public OvertimeViewModel[] Overtimes { get; set; }
		public bool IsDayOff { get; set; }

		public ShiftViewModel Shift { get; set; }
		public bool HasOvertime { get; set; }
		public IList<OccupancyViewModel> SeatBookings { get; set; }
		public BankHolidayCalendarViewModel BankHolidayCalendarInfo { get; set; }
	}

	public class ShiftViewModel
	{
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string Color { get; set; }
		public string TimeSpan { get; set; }
		public string WorkingHours { get; set; }
	}

	public class AbsenceViewModel
	{
		public string Name { get; set; }
		public string ShortName { get; set; }
		public bool IsFullDayAbsence { get; set; }
		public string Color { get; set; }
	}

	public class OvertimeViewModel
	{
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string Color { get; set; }
	}
}