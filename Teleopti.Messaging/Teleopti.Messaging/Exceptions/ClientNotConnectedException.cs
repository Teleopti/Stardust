using System;
using System.Runtime.Serialization;

namespace Teleopti.Messaging.Exceptions
{
    [Serializable]
    public class ClientNotConnectedException : MessageBrokerException
    {

        public ClientNotConnectedException()
        {

        }

        public ClientNotConnectedException(string message) : base(message)
        {

        }

        public ClientNotConnectedException(string message, Exception innerException) : base(message, innerException)
        {

        }

        protected ClientNotConnectedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

    }
}
