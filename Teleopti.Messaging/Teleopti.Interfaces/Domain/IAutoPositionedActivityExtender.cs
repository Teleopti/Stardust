using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Puts activitylayers to auto positioned place.
    /// This layer will be centered in the longest projected
    /// time of contract time.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 20011-05-24
    /// </remarks>
    public interface IAutoPositionedActivityExtender : IWorkShiftExtender
    {
        /// <summary>
        /// Gets or sets the start segment.
        /// </summary>
        /// <value>The start segment.</value>
        TimeSpan StartSegment { get; set; }

        /// <summary>
        /// Gets or sets the auto position interval segment.
        /// </summary>
        /// <value>The auto position interval segment.</value>
        TimeSpan AutoPositionIntervalSegment { get; set; }
    }
}