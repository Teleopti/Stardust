using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// IRecurrentMeeting interface
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// Created date: 12/16/2008
    /// </remarks>
    public interface IRecurrentMeeting : IAggregateEntity, ICloneableEntity<IRecurrentMeeting>
    {
        /// <summary>
        /// Gets or sets the recurring date.
        /// </summary>
        /// <value>The recurring date.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 1/7/2009
        /// </remarks>
        DateTime RecurringDate { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>The start time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 1/7/2009
        /// </remarks>
        TimeSpan StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>The end time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 1/7/2009
        /// </remarks>
        TimeSpan EndTime { get; set; }
    }
}
