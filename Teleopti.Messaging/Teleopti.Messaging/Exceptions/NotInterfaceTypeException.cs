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

        protected NotInterfaceTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}