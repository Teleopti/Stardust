using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The Multicast Address information.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-07
    /// </remarks>
    public interface IAddressInformation : ISerializable
    {
        /// <summary>
        /// Gets or sets the address id.
        /// </summary>
        /// <value>The address id.</value>
        int AddressId { get; set; }

        /// <summary>
        /// Gets or sets the multicast address.
        /// </summary>
        /// <value>The multicast address.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        string Address { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        int Port { get; set; }

        /// <summary>
        /// Gets or sets the time to live.
        /// </summary>
        /// <value>The time to live.</value>
        int TimeToLive { get; set; }
    }

    ///<summary>
    /// The Message Information
    ///</summary>
    public interface IMessageInformation : IAddressInformation
    {
        /// <summary>
        /// Gets or sets the subscriber id.
        /// </summary>
        /// <value>The subscriber id.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        Guid SubscriberId { get; set; }
        /// <summary>
        /// Gets or sets the broadcast direction.
        /// </summary>
        /// <value>The broadcast direction.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        byte[] Package { get; set; }
        /// <summary>
        /// Gets or sets the event message.
        /// </summary>
        /// <value>The event message.</value>
        IEventMessage EventMessage { get; set; }
    }

}