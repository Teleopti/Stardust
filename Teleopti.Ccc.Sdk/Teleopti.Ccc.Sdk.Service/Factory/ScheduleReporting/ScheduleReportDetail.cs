using System;

namespace Teleopti.Ccc.Sdk.WcfService.Factory.ScheduleReporting
{
    /// <summary>
    /// Detail level for schedule report and export
    /// </summary>
    [Serializable]
    internal enum ScheduleReportDetail
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