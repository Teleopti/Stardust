using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Win.Forecasting.Forms;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    /// <summary>
    /// Extension of eventargs to synchronize working interval between tabs and calendar
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-25
    /// </remarks>
    public class WorkingIntervalChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the new working interval.
        /// </summary>
        /// <value>The new working interval.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public WorkingInterval NewWorkingInterval { get; set; }

        /// <summary>
        /// Gets or sets the new start date.
        /// </summary>
        /// <value>The new start date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public DateOnly NewStartDate { get; set; }

        /// <summary>
        /// Gets or sets the new time of day.
        /// </summary>
        /// <value>The new time of day.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-15
        /// </remarks>
        public TimeSpan NewTimeOfDay { get; set; }
    }
}
