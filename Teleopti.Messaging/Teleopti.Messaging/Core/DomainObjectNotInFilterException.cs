using System;
using System.Runtime.Serialization;

namespace Teleopti.Messaging.Core
{
    [Serializable]
    public class DomainObjectNotInFilterException : Exception
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
