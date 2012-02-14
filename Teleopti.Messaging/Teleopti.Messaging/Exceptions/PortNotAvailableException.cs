using System;
using System.Runtime.Serialization;

namespace Teleopti.Messaging.Exceptions
{
    [Serializable]
    public class PortNotAvailableException : MessageBrokerException
    {
        
        public PortNotAvailableException()
        {
        }

        public PortNotAvailableException(string message) : base(message)
        {
        }

        public PortNotAvailableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PortNotAvailableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }


    }
}
