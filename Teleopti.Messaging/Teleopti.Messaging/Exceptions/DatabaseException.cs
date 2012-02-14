using System;
using System.Runtime.Serialization;

namespace Teleopti.Messaging.Exceptions
{
    [Serializable]
    public class DatabaseException : MessageBrokerException
    {

        public DatabaseException()
        {

        }

        public DatabaseException(string message) : base(message)
        {

        }

        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {

        }

        protected DatabaseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

    }
}
