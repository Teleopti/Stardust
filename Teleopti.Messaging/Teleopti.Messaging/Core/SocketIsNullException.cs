using System;
using System.Runtime.Serialization;

namespace Teleopti.Messaging.Core
{
    [Serializable]
    public class SocketIsNullException : Exception
    {

        public SocketIsNullException()
        {

        }

        public SocketIsNullException(string message) : base(message)
        {

        }

        public SocketIsNullException(string message, Exception innerException) : base(message, innerException)
        {

        }


        protected SocketIsNullException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }


    }
}
