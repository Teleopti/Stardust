namespace Teleopti.Interfaces.MessageBroker.Core
{
    ///<summary>
    /// Messaging Protocol
    ///</summary>
    public enum MessagingProtocol 
    {
        ///<summary>
        /// Multicast 
        ///</summary>
        Multicast,
        ///<summary>
        /// TCP/IP
        ///</summary>
        TcpIP,
        ///<summary>
        /// UDP
        ///</summary>
        Udp,
        /// <summary>
        /// Client initiated TCP/IP
        /// </summary>
        ClientTcpIP,
    }
}
