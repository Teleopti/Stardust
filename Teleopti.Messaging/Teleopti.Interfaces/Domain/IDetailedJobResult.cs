using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// 
    ///</summary>
    public interface IDetailedJobResult: IAggregateRoot
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
