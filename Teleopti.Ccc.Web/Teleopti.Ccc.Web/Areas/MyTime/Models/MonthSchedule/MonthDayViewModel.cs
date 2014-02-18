using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule
{
    public class MonthDayViewModel
    {
        public DateTime Date { get; set; }
        public string FixedDate { get; set; }
        public bool IsWorkingDay { get; set; }
        public string DisplayColor { get; set; }
        public AbsenceViewModel Absence { get; set; }
	    public bool IsDayOff { get; set; }
    }

    public class AbsenceViewModel
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
}