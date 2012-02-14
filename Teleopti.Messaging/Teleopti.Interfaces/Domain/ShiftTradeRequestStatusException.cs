using System;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Exception type for exceptions regarding invalid status modes for shift trade requests
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-09-07
    /// </remarks>
    [Serializable]
    public class ShiftTradeRequestStatusException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftTradeRequestStatusException"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-07
        /// </remarks>
        public ShiftTradeRequestStatusException()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftTradeRequestStatusException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-07
        /// </remarks>
        public ShiftTradeRequestStatusException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftTradeRequestStatusException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-07
        /// </remarks>
        public ShiftTradeRequestStatusException(string message, Exception exception)
            : base(message, exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftTradeRequestStatusException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        /// </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
        /// </exception>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-07
        /// </remarks>
        protected ShiftTradeRequestStatusException(SerializationInfo info, StreamingContext context) : base(info,context)
        {
        }
    }
}