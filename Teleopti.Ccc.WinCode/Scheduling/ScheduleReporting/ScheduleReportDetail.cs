using System;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting
{
    /// <summary>
    /// Detail level for schedule report and export
    /// </summary>
    [Serializable]
    public enum ScheduleReportDetail
    {
        /// <summary>
        /// No details
        /// </summary>
        None,
        /// <summary>
        /// Details marked as break
        /// </summary>
        Break,
        /// <summary>
        /// All details
        /// </summary>
        All
    }
}
