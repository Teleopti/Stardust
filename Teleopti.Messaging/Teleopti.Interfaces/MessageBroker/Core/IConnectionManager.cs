using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    ///<summary>
    /// The connection manager, keeps track of client tcp ip connections.
    ///</summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ip")]
    public interface IClientTcpIpManager
    {
        
        /// <summary>
        /// Adds the listner.
        /// </summary>
        int AddListener(string address);
        
        /// <summary>
        /// Removes the listner.
        /// </summary>
        /// <param name="port">The port.</param>
        void RemoveListener(int port);

        /// <summary>
        /// Sends the package.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="value">The value.</param>
        void SendPackage(string address, int port, byte[] value);

        /// <summary>
        /// Removes the listener.
        /// </summary>
        /// <param name="eventSubscriber">The event subscriber.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 08/05/2010
        /// </remarks>
        void RemoveListener(IEventSubscriber eventSubscriber);

    }
}