using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// The Real Time Adherence (RTA) state an agent can be in on a specific ACD platform.
    /// </summary>
    /// <remarks>
    /// Created by: Jonas N
    /// Created date: 2008-10-03
    /// </remarks>
    public interface IRtaState : IAggregateEntity, ICloneableEntity<IRtaState>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2008-10-03
        /// </remarks>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the state code. The state id or code from a specific ACD platform.
        /// </summary>
        /// <value>The state code.</value>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2008-10-03
        /// </remarks>
        string StateCode { get; set; }

        /// <summary>
        /// Gets the state group that the state belongs to.
        /// </summary>
        /// <value>The state group.</value>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2008-10-03
        /// </remarks>
        IRtaStateGroup StateGroup { get; }

        /// <summary>
        /// Gets the platform type id. A static GUID to track which platform this state comes from.
        /// </summary>
        /// <value>The platform type id.</value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-10-13
        /// </remarks>
        Guid PlatformTypeId { get; }
    }
}