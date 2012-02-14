using System;
using System.Drawing;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Alarm type for real time adherence data
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-11-06
    /// </remarks>
    public interface IAlarmType : IPayload
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        Description Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the display color of the Payload.
        /// </summary>
        /// <value>The color of the display.</value>
        Color DisplayColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the threshold time.
        /// </summary>
        /// <value>The threshold time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-12
        /// </remarks>
        TimeSpan ThresholdTime { get; set; }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The mode.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-14
        /// </remarks>
        AlarmTypeMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the staffing effect.
        /// </summary>
        /// <value>The staffing effect.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-12-17
        /// </remarks>
        double StaffingEffect { get; set; }
    }
}