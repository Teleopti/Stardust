using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// Detailed information for a job result
    ///</summary>
    public interface IJobResultDetail : IAggregateEntity
    {
        ///<summary>
        /// The timestamp for this detail.
        ///</summary>
        DateTime Timestamp { get; }

        ///<summary>
        /// The stack trace of the inner exception.
        ///</summary>
        string InnerExceptionStackTrace { get; }

        ///<summary>
        /// The message of the inner exception.
        ///</summary>
        string InnerExceptionMessage { get; }

        ///<summary>
        /// The stack trace of the exception.
        ///</summary>
        string ExceptionStackTrace { get; }

        ///<summary>
        /// The message of the exception.
        ///</summary>
        string ExceptionMessage { get; }

        ///<summary>
        /// The message.
        ///</summary>
        string Message { get; }

        ///<summary>
        /// The level of detail.
        ///</summary>
        DetailLevel DetailLevel { get; }
    }
}