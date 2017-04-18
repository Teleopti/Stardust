
using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting
{
    public interface IReportData
    {
        string ActivityName { get; set; }
        double ScheduledTime { get; set; }
        DateTime ScheduledDate { get; set; }
    }
}
