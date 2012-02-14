using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Domain.Security
{
    /// <summary>
    /// Exception thrown regarding not sufficient privileges to run a command, or a user function.
    /// </summary>
    [Serializable]
    public class InsufficientPrivilegeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InsufficientPrivilegeException"/> class.
        /// </summary>
        public InsufficientPrivilegeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsufficientPrivilegeException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public InsufficientPrivilegeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsufficientPrivilegeException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InsufficientPrivilegeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsufficientPrivilegeException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"></see> is zero (0). </exception>
        /// <exception cref="T:System.ArgumentNullException">The info parameter is null. </exception>
        protected InsufficientPrivilegeException(SerializationInfo info,
                                      StreamingContext context) : base(info, context)
        {
        }
    }
}
