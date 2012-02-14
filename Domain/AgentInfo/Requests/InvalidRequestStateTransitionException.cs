using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    [Serializable]
    public class InvalidRequestStateTransitionException : InvalidOperationException
    {
        public InvalidRequestStateTransitionException(string message) : base(message){}

        public InvalidRequestStateTransitionException(string message, Exception innerException) : base(message,innerException) { }

        protected InvalidRequestStateTransitionException(SerializationInfo info,StreamingContext context) :base(info,context) {}

        public InvalidRequestStateTransitionException(){}
    }
}