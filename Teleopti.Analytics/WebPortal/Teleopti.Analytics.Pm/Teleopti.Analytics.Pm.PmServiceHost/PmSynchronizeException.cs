using System;
using System.Runtime.Serialization;

namespace Teleopti.Analytics.PM.PMServiceHost
{
    [Serializable]
    public class PmSynchronizeException : Exception
    {
        public PmSynchronizeException(string message)
            : base(message)
        {
        }

        protected PmSynchronizeException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
