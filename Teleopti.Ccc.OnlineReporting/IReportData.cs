
using System;

namespace Teleopti.Ccc.OnlineReporting
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IReportData : IComparable<IReportData>
    {
        string ActivityName { get; set; }
        double ScheduledTime { get; set; }
        DateTime ScheduledDate { get; set; }
    }
}
