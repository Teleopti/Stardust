#region Imports

using System;

#endregion

namespace Teleopti.Interfaces.Domain
{

    /// <summary>
    /// Represents a IMyReportAdherenceInfo
    /// </summary>
    public interface IMyReportAdherenceInfo : IEquatable<IMyReportAdherenceInfo>
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        DateTime AdherenceDate { get; set; }

        /// <summary>
        /// Gets or sets the hour.
        /// </summary>
        /// <value>The hour.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        int Hour { get; set; }

        /// <summary>
        /// Gets or sets the segment.
        /// </summary>
        /// <value>The segment.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        int Segment { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        double Value { get; set; }    
    }

}
