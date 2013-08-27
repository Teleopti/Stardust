using System;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.Infrastructure
{
    [Serializable]
    public class DomainObjectNotInFilterException : MessageBrokerException
    {
        public DomainObjectNotInFilterException(string message) : base(message)
        {
        }

        protected DomainObjectNotInFilterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}