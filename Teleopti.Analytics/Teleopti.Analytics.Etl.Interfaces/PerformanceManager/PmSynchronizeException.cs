using System;
using System.Runtime.Serialization;

namespace Teleopti.Analytics.Etl.Interfaces.PerformanceManager
{
    [Serializable]
    public class PmSynchronizeException : Exception
    {
        public PmSynchronizeException(string message)
            : base(message)
        {
        }

        public PmSynchronizeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PmSynchronizeException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}