using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The TCP IP Sender instance.
    /// </summary>
    public interface ITcpSender : IDisposable
    {

        ///<summary>
        /// Connect to an end point
        ///</summary>
        ///<param name="remoteEP"></param>
        void Connect(IPEndPoint remoteEP);

        ///<summary>
        /// connect to an endpoint via address and port
        ///</summary>
        ///<param name="address"></param>
        ///<param name="port"></param>
        void Connect(IPAddress address, int port);

        ///<summary>
        /// connect to an endpont via hostname and port.
        ///</summary>
        ///<param name="hostName"></param>
        ///<param name="port"></param>
        void Connect(string hostName, int port);

        /// <summary>
        /// Connects the specified ip addresses.
        /// </summary>
        /// <param name="ipAddresses">The ip addresses.</param>
        /// <param name="port">The port.</param>
        void Connect(IPAddress[] ipAddresses, int port);

        /// <summary>
        /// Closes this connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        NetworkStream GetStream();
        /// <summary>
        /// Gets the available.
        /// </summary>
        /// <value>The available.</value>
        int Available { get; }
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        Socket Client { get; set; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="ITcpSender"/> is connected.
        /// </summary>
        /// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
        bool Connected { get; }
        /// <summary>
        /// Gets or sets a value indicating whether [exclusive address use].
        /// </summary>
        /// <value><c>true</c> if [exclusive address use]; otherwise, <c>false</c>.</value>
        bool ExclusiveAddressUse { get; set; }
        /// <summary>
        /// Gets or sets the state of the linger.
        /// </summary>
        /// <value>The state of the linger.</value>
        LingerOption LingerState { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [no delay].
        /// </summary>
        /// <value><c>true</c> if [no delay]; otherwise, <c>false</c>.</value>
        bool NoDelay { get; set; }
        /// <summary>
        /// Gets or sets the size of the receive buffer.
        /// </summary>
        /// <value>The size of the receive buffer.</value>
        int ReceiveBufferSize { get; set; }
        /// <summary>
        /// Gets or sets the receive timeout.
        /// </summary>
        /// <value>The receive timeout.</value>
        int ReceiveTimeout { get; set; }
        /// <summary>
        /// Gets or sets the size of the send buffer.
        /// </summary>
        /// <value>The size of the send buffer.</value>
        int SendBufferSize { get; set; }
        /// <summary>
        /// Gets or sets the send timeout.
        /// </summary>
        /// <value>The send timeout.</value>
        int SendTimeout { get; set; }
        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        int Port { get; set; }
        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>The address.</value>
        string Address { get; set; }
        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        void Shutdown();
    }
}