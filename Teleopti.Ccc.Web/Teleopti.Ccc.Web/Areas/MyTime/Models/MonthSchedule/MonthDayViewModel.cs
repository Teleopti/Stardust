using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule
{
    public class MonthDayViewModel
    {
        public DateTime Date { get; set; }
        public string FixedDate { get; set; }
        public AbsenceViewModel Absence { get; set; }
	    public bool IsDayOff { get; set; }

	    public ShiftViewModel Shift { get; set; }
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
    }
}