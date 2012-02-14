using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The UDP Sender interface.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 15/05/2010
    /// </remarks>
    public interface IUdpSender : IDisposable
    {
        
        /// <summary>
        /// Closes this instance.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        void Close();

        /// <summary>
        /// Connects the specified end point.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "endPoint")]
        void Connect(IPEndPoint endPoint);

        /// <summary>
        /// Connects the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        void Connect(IPAddress address, int port);

        /// <summary>
        /// Connects the specified host name.
        /// </summary>
        /// <param name="hostName">Name of the host.</param>
        /// <param name="port">The port.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void Connect(string hostName, int port);

        /// <summary>
        /// Drops the multicast group.
        /// </summary>
        /// <param name="multicastAddress">The multicast address.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        void DropMulticastGroup(IPAddress multicastAddress);
        
        /// <summary>
        /// Drops the multicast group.
        /// </summary>
        /// <param name="multicastAddress">The multicast address.</param>
        /// <param name="index">The index.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        void DropMulticastGroup(IPAddress multicastAddress, int index);
        
        /// <summary>
        /// Joins the multicast group.
        /// </summary>
        /// <param name="multicastAddress">The multicast address.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        void JoinMulticastGroup(IPAddress multicastAddress);
        
        /// <summary>
        /// Joins the multicast group.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="multicastAddress">The multicast address.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        void JoinMulticastGroup(int index, IPAddress multicastAddress);
        
        /// <summary>
        /// Joins the multicast group.
        /// </summary>
        /// <param name="multicastAddress">The multicast address.</param>
        /// <param name="timeToLive">The time to live.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        void JoinMulticastGroup(IPAddress multicastAddress, int timeToLive);
        
        /// <summary>
        /// Joins the multicast group.
        /// </summary>
        /// <param name="multicastAddress">The multicast address.</param>
        /// <param name="localAddress">The local address.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        void JoinMulticastGroup(IPAddress multicastAddress, IPAddress localAddress);
        
        /// <summary>
        /// Receives the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        int Receive(byte[] buffer, int offset, int size);
        
        /// <summary>
        /// Sends the specified datagram.
        /// </summary>
        /// <param name="datagram">The datagram.</param>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        int Send(byte[] datagram, int bytes);

        /// <summary>
        /// Sends the specified datagram.
        /// </summary>
        /// <param name="datagram">The datagram.</param>
        /// <param name="bytes">The bytes.</param>
        /// <param name="endPoint">The end point.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "endPoint")]
        int Send(byte[] datagram, int bytes, IPEndPoint endPoint);

        /// <summary>
        /// Sends the specified datagram.
        /// </summary>
        /// <param name="datagram">The datagram.</param>
        /// <param name="bytes">The bytes.</param>
        /// <param name="hostName">Name of the host.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        int Send(byte[] datagram, int bytes, string hostName, int port);
        
        /// <summary>
        /// Gets the available.
        /// </summary>
        /// <value>The available.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        int Available { get; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        Socket Client { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [dont fragment].
        /// </summary>
        /// <value><c>true</c> if [dont fragment]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Dont")]
        bool DontFragment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable broadcast].
        /// </summary>
        /// <value><c>true</c> if [enable broadcast]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        bool EnableBroadcast { get; set; }
        
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
        /// Gets or sets a value indicating whether [multicast loopback].
        /// </summary>
        /// <value><c>true</c> if [multicast loopback]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        bool MulticastLoopback { get; set; }

        /// <summary>
        /// Gets or sets the TTL.
        /// </summary>
        /// <value>The TTL.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ttl")]
        short Ttl { get; set; }

    }
}