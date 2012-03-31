namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Provide choices regarding importing forecasts
    /// </summary>
    public enum ImportForecastsOptionsDto
    {
        /// <summary>
        /// Import workload only
        /// </summary>
        ImportWorkload,
        /// <summary>
        /// Import staffing only
        /// </summary>
        ImportStaffing,
        /// <summary>
        /// Import both workload and staffing
        /// </summary>
        ImportWorkloadAndStaffing
    }
}
