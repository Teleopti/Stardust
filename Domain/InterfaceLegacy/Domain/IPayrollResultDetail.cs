using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// Detailed information for a payroll result.
    ///</summary>
    public interface IPayrollResultDetail : IAggregateEntity
    {
        ///<summary>
        /// The stack trace of the inner exception. (null if no inner exception)
        ///</summary>
        string InnerExceptionStackTrace { get; }

        ///<summary>
        /// The message of the inner exception. (null if no inner exception)
        ///</summary>
        string InnerExceptionMessage { get; }

        ///<summary>
        /// The stack trace of the exception. (null if no exception)
        ///</summary>
        string ExceptionStackTrace { get; }

        ///<summary>
        /// The message of the exception. (null if no exception)
        ///</summary>
        string ExceptionMessage { get; }

        ///<summary>
        /// The message for this detail.
        ///</summary>
        string Message { get; }

        ///<summary>
        /// The level of this detail.
        ///</summary>
        DetailLevel DetailLevel { get; }

        ///<summary>
        /// The timestamp of this detail.
        ///</summary>
        DateTime Timestamp { get; }
    }
}