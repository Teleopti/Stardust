using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The custom UDP listener interface.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 15/05/2010
    /// </remarks>
    public interface ICustomUdpListener : IDisposable
    {
        /// <summary>
        /// Accepts the socket.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        Socket AcceptSocket();
        /// <summary>
        /// Accepts the UDP sender.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        IUdpSender AcceptUdpSender();
        /// <summary>
        /// Accepts the multicast sender.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        IUdpSender AcceptMulticastSender();
        /// <summary>
        /// Pendings this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        bool Pending();
        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        void Start();
        /// <summary>
        /// Stops this instance.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Stop")]
        void Stop();
        /// <summary>
        /// Gets or sets a value indicating whether [exclusive address use].
        /// </summary>
        /// <value><c>true</c> if [exclusive address use]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        bool ExclusiveAddressUse { get; set; }
        /// <summary>
        /// Gets the local endpoint.
        /// </summary>
        /// <value>The local endpoint.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        EndPoint LocalEndpoint { get; }
        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <value>The server.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        Socket Server { get; }
    }
}