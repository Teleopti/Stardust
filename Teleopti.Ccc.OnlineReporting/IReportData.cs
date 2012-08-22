
using System;

namespace Teleopti.Ccc.OnlineReporting
{
    public interface IReportData
    {
        string ActivityName { get; set; }
        double ScheduledTime { get; set; }
        DateTime ScheduledDate { get; set; }
    }
}
