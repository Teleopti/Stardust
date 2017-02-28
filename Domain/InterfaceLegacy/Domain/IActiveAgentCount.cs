using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// The number of active agents during an interval
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-09-19
    /// </remarks>
    public interface IActiveAgentCount
    {
        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>The interval.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-19
        /// </remarks>
        DateTime Interval { get; set; }

        /// <summary>
        /// Gets or sets the number of active agents.
        /// </summary>
        /// <value>The number of active agents.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-19
        /// </remarks>
        int ActiveAgents { get; set; }
    }
}