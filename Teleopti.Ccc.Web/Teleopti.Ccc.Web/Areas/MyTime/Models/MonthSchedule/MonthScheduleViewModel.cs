using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule
{
    public class MonthScheduleViewModel
    {
        public IEnumerable<MonthDayViewModel> ScheduleDays { get; set; }

        public DateTime CurrentDate { get; set; }

        public string FixedDate { get; set; }
        public IEnumerable<Description> DayHeaders { get; set; }
    }
}