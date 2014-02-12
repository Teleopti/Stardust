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
        public bool IsNotWorkingDay { get; set; }
        public string Absence { get; set; }
    }
}