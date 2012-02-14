using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The socket information
    /// </summary>
    public interface ISocketInfo : ISerializable
    {
        /// <summary>
        /// The address, e.g. 172.22.1.31.
        /// </summary>
        string Address { get; set; }
        /// <summary>
        /// The port, e.g. 9090
        /// </summary>
        int Port { get; set; }
        /// <summary>
        /// Time to live, default 1. Used for Router hops.
        /// </summary>
        int TimeToLive { get; set; }
        /// <summary>
        /// Gets or sets the client throttle.
        /// </summary>
        /// <value>The client throttle.</value>
        int ClientThrottle { get; set; }
        /// <summary>
        /// The socket used for communication
        /// </summary>
        Socket Socket { get; set; }
        /// <summary>
        /// The acctual IPAddress, a C# wrapper around Winsock 2.0.
        /// </summary>
        IPAddress IPAddress { get; set; }
        /// <summary>
        /// The IPEndpoint, receiving information.
        /// </summary>
        IPEndPoint IPEndpoint { get; set; }

    }
}