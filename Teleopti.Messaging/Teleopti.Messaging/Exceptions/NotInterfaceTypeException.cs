using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

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