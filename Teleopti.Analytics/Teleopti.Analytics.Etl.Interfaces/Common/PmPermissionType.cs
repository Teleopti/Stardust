namespace Teleopti.Analytics.Etl.Interfaces.Common
{
    /// <summary>
    /// The permissions types for Performance Manager.
    /// </summary>
    public enum PmPermissionType
    {
        /// <summary>
        /// Permission denied
        /// </summary>
        None = 0,

        /// <summary>
        /// Permissions to view Performance Manager reports.
        /// </summary>
        GeneralUser = 81,

        /// <summary>
        /// Permissions to create Performance Manager reports.
        /// </summary>
        ReportDesigner = 85
    }
}