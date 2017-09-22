using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Messaging.Exceptions
{
    [Serializable]
    public class BrokerNotInstantiatedException : MessageBrokerException
    {

        public BrokerNotInstantiatedException()
        {

        }

        public BrokerNotInstantiatedException(string message) : base(message)
        {

        }

        public BrokerNotInstantiatedException(string message, Exception innerException) : base(message, innerException)
        {

        }

        protected BrokerNotInstantiatedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

    }
}