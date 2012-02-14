using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// IMyReportVisualPayloadInfo interface
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// Created date: 11/24/2008
    /// </remarks>
    public interface IMyReportVisualPayloadInfo : IEquatable<IMyReportVisualPayloadInfo>
    {
        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>The start date time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>The end date time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        DateTime EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the color of the activity.
        /// </summary>
        /// <value>The color of the activity.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        Color ActivityColor { get; set; }

        /// <summary>
        /// Gets or sets the activity.
        /// </summary>
        /// <value>The activity.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        string Activity { get; set; }
    }
}
