using System;

namespace Teleopti.Ccc.Sdk.ServiceBus.Messages
{
    public class CustomErrorMessage
    {
        public object Message { get; set; }

        public Uri Destination { get; set; }

        public string MessageId { get; set; }

        public string TransportMessageId { get; set; }

        public Uri Source { get; set; }
    }
}