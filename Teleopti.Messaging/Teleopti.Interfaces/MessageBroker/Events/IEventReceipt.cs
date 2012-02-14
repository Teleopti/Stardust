using System;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.MessageBroker.Events
{
    /// <summary>
    /// Event Receipt.
    /// </summary>
    public interface IEventReceipt : ISerializable
    {
        /// <summary>
        /// Guid, a unique Id for this Receipt.
        /// </summary>
        Guid ReceiptId { get; set; }
        /// <summary>
        /// Event Id
        /// </summary>
        Guid EventId { get; set; }
        /// <summary>
        /// The process id that received the message
        /// </summary>
        int ProcessId { get; set; }
        /// <summary>
        /// Who created this receipt.
        /// </summary>
        string ChangedBy { get; set; }
        /// <summary>
        /// When was this receipt created.
        /// </summary>
        DateTime ChangedDateTime { get; set; }
    }
}