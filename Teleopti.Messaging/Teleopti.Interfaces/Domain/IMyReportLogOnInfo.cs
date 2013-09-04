using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// IMyReportLogOnInfo interface
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// Created date: 11/24/2008
    /// </remarks>
    public interface IMyReportLogOnInfo
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        DateTime LogOnDate { get; set; }

        /// <summary>
        /// Gets or sets the cti time.
        /// </summary>
        /// <value>The cti time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        string CtiTime { get; set; }

        /// <summary>
        /// Gets or sets the logged in time.
        /// </summary>
        /// <value>The logged in time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        string LoggedInTime { get; set; }

        /// <summary>
        /// Gets or sets the idle time.
        /// </summary>
        /// <value>The idle time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        string IdleTime { get; set; }

        /// <summary>
        /// Gets or sets the available time.
        /// </summary>
        /// <value>The available time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        string AvailableTime { get; set; }

    }
}
