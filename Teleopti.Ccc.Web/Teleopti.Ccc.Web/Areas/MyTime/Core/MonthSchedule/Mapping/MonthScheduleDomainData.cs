using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping
{
    public class MonthScheduleDomainData
    {
        public DateOnly CurrentDate { get; set; }
        public IEnumerable<MonthScheduleDayDomainData> Days { get; set; }
    }

    public class MonthScheduleDayDomainData
    {
        public IScheduleDay ScheduleDay { get; set; }
    }
}