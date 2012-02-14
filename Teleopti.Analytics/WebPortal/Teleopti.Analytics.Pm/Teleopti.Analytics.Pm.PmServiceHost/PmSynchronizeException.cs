using System;
using System.Runtime.Serialization;

namespace Teleopti.Analytics.PM.PMServiceHost
{
    [Serializable]
    public class PmSynchronizeException : Exception
    {
        public PmSynchronizeException()
        {
        }

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
