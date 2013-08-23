using System;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.Infrastructure
{
    [Serializable]
    public class DomainObjectNotInFilterException : MessageBrokerException
    {

        public DomainObjectNotInFilterException()
        {
        }

        public DomainObjectNotInFilterException(string message) : base(message)
        {
        }

        public DomainObjectNotInFilterException(string message, Exception innerException) : base(message, innerException)
        {
        }


        protected DomainObjectNotInFilterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}