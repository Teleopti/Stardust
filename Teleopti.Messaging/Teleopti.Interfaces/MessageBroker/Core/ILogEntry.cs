using System;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// Log Entry.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-07
    /// </remarks>
    public interface ILogEntry : ISerializable
    {
        /// <summary>
        /// The Id of the log entry.
        /// </summary>
        Guid LogId { get; set; }

        /// <summary>
        /// The process id, at the end point, of the subscriber's client instance.
        /// </summary>
        int ProcessId { get; set; }

        /// <summary>
        /// Describe the error or provide information.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// The short form of an exception. 50 Characters long.
        /// </summary>
        string Exception { get; set; }

        /// <summary>
        /// The short form of the message. 50 Characters long.
        /// </summary>
        string Message { get; set; }

        /// <summary>
        /// The stack traces of the exception logged. 50 Characters long.
        /// </summary>
        string StackTrace { get; set; }

        /// <summary>
        /// The log entry was generated/created/changed by a user or
        /// a program namespace, restricted to 10 characters so it needs
        /// to be abbreviated.
        /// </summary>
        string ChangedBy { get; set; }

        /// <summary>
        /// When the log entry was generated/created/changed.
        /// </summary>
        DateTime ChangedDateTime { get; set; }

        /// <summary>
        /// Override of the ToString method
        /// </summary>
        /// <returns></returns>
        String ToString();

    }
}