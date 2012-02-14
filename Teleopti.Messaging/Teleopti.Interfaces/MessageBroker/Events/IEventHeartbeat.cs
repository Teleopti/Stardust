using System;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.MessageBroker.Events
{
    /// <summary>
    /// The heart beat message signaling that the client is still alive.
    /// </summary>
    public interface IEventHeartbeat : ISerializable
    {
        /// <summary>
        /// Heartbeat id.
        /// </summary>
        Guid HeartbeatId { get; set; }
        /// <summary>
        /// Subscriber id.
        /// </summary>
        Guid SubscriberId { get; set; }
        /// <summary>
        /// The process id of process sending the heartbeat.
        /// </summary>
        int ProcessId { get; set; }
        /// <summary>
        /// Who is logged in, who is running this process.
        /// </summary>
        string ChangedBy { get; set; }
        /// <summary>
        /// When was this heartbeat created
        /// </summary>
        DateTime ChangedDateTime { get; set; }
    }
}