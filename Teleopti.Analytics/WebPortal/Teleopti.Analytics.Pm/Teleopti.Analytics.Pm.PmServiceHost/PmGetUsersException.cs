using System;
using System.Runtime.Serialization;

namespace Teleopti.Analytics.PM.PMServiceHost
{
    [Serializable]
    public class PmGetUsersException : Exception
    {
        public PmGetUsersException(string message)
            : base(message)
        {
        }

        protected PmGetUsersException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
