using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    ///<summary>
    /// Custom Tcp Listner
    ///</summary>
    public interface ICustomTcpListener : IDisposable
    {
        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        int Port { get; set; }
        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        string Address { get; set; }
        /// <summary>
        /// Accepts the socket.
        /// </summary>
        /// <returns></returns>
        Socket AcceptSocket();
        /// <summary>
        /// Begins the accept TCP sender.
        /// </summary>
        /// <returns></returns>
        IAsyncResult BeginAcceptTcpSender(AsyncCallback callback);
        /// <summary>
        /// Ends the accept TCP sender.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        /// <returns></returns>
        ITcpSender EndAcceptTcpSender(IAsyncResult asyncResult);
        /// <summary>
        /// Pendings this instance.
        /// </summary>
        /// <returns></returns>
        bool Pending();
        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();
        /// <summary>
        /// Starts the specified backlog.
        /// </summary>
        /// <param name="backlog">The backlog.</param>
        void Start(int backlog);
        /// <summary>
        /// Stops this instance.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Stop")]
        void Stop();
        /// <summary>
        /// Gets or sets a value indicating whether [exclusive address use].
        /// </summary>
        /// <value><c>true</c> if [exclusive address use]; otherwise, <c>false</c>.</value>
        bool ExclusiveAddressUse { get; set; }
        /// <summary>
        /// Gets the local endpoint.
        /// </summary>
        /// <value>The local endpoint.</value>
        EndPoint LocalEndpoint { get; }
        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <value>The server.</value>
        Socket Server { get; }
    }
}