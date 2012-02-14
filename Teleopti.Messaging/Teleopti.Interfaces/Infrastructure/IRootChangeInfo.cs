﻿using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.Infrastructure
{
    /// <summary>
    /// Holds info about what and how a root has been persisted
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-06-26
    /// </remarks>
    public interface IRootChangeInfo
    {
        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-06-12
        /// </remarks>
        DomainUpdateType Status { get; }

        /// <summary>
        /// Gets the root.
        /// </summary>
        /// <value>The root.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-06-12
        /// </remarks>
        object Root { get; }
    }
}