using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Agent state information from external sources
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-11-06
    /// </remarks>
    public interface IExternalAgentState
    {
        /// <summary>
        /// Gets the time stamp.
        /// </summary>
        /// <value>The time stamp.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-06
        /// </remarks>
        DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets the time agent has been in this state.
        /// </summary>
        /// <value>The time in state.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-06
        /// </remarks>
        TimeSpan TimeInState { get; set; }

        /// <summary>
        /// Gets the state code.
        /// </summary>
        /// <value>The state code.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-06
        /// </remarks>
        string StateCode { get; set; }

        /// <summary>
        /// Gets the external log on.
        /// </summary>
        /// <value>The external log on.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-06
        /// </remarks>
        string ExternalLogOn { get; set; }

        /// <summary>
        /// Gets the platform type id.
        /// </summary>
        /// <value>The platform type id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-06
        /// </remarks>
        Guid PlatformTypeId { get; set; }

        /// <summary>
        /// Gets the data source id. Use togheter with ExternalLogon to map with correct agent.
        /// </summary>
        /// <value>The data source id.</value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-12-11
        /// </remarks>
        int DataSourceId { get; }

        /// <summary>
        /// Gets the batch id that keeps track of state changes that belongs to the same state snapshot from the platform. 
        /// When batch id is equal to SqlDateTime.MinValue.Value then the state change is not part of snapshot.
        /// </summary>
        /// <value>The batch id.</value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-12-11
        /// </remarks>
        DateTime BatchId { get; }

        /// <summary>
        /// Gets a value indicating whether this instance belongs to a snapshot.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is snapshot; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-12-17
        /// </remarks>
        bool IsSnapshot { get; set; }
    }
}