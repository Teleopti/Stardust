using System;
using System.Runtime.Serialization;

namespace Teleopti.Messaging.Core
{
    [Serializable]
    public class BrokerNotInstantiatedException : Exception
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
