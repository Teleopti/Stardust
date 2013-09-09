using System;

namespace Teleopti.Interfaces.Infrastructure
{
    /// <summary>
    /// Holds info of what module it is 
    /// that persist data.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-07-01
    /// </remarks>
    public interface IMessageBrokerIdentifier
    {
        /// <summary>
        /// Gets the module id.
        /// Used by MB to decide if it's the module itself that made the persist.
        /// </summary>
        /// <value>The module id.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-07-01
        /// </remarks>
        Guid InstanceId { get; }
    }
}