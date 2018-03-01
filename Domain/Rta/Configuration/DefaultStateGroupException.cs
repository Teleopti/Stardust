using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Domain.Rta.Configuration
{
    [Serializable]
    public class DefaultStateGroupException : Exception
    {
        public DefaultStateGroupException()
        {
        }

        public DefaultStateGroupException(string message)
            : base(message)
        {
        }

        public DefaultStateGroupException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DefaultStateGroupException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}