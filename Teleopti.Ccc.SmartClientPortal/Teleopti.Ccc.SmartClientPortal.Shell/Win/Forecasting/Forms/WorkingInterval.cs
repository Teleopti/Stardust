using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    /// <summary>
    /// Enum containing the different intervals to work with.
    /// Should have a better name and be moved down to wincode project.
    /// </summary>
    [Serializable]
    public enum WorkingInterval
    {
        /// <summary>
        /// Working with the complete original selection
        /// </summary>
        Selection,
        /// <summary>
        /// Working with a custom interval (for example a selection of days)
        /// </summary>
        Custom,
        /// <summary>
        /// Month
        /// </summary>
        Month,
        /// <summary>
        /// Week
        /// </summary>
        Week,
        /// <summary>
        /// Day
        /// </summary>
        Day,
        /// <summary>
        /// Intraday
        /// </summary>
        Intraday
    }
}
