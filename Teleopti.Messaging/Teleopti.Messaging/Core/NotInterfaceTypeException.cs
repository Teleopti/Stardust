using System;
using System.Runtime.Serialization;

namespace Teleopti.Messaging.Core
{
    [Serializable]
    public class NotInterfaceTypeException : Exception
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


