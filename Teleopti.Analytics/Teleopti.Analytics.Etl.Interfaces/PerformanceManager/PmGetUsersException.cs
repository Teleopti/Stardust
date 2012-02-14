using System;
using System.Runtime.Serialization;

namespace Teleopti.Analytics.Etl.Interfaces.PerformanceManager
{
    [Serializable]
    public class PmGetUsersException : Exception
    {
        public PmGetUsersException()
        {
        }

        public PmGetUsersException(string message)
            : base(message)
        {
        }

        public PmGetUsersException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PmGetUsersException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
