using System;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Messaging.Exceptions
{
    [Serializable]
    public class NotInterfaceTypeException : MessageBrokerException
    {

        public NotInterfaceTypeException()
        {
        }

        public NotInterfaceTypeException(string message) : base(message)
        {
        }

        public NotInterfaceTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }


        protected NotInterfaceTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}