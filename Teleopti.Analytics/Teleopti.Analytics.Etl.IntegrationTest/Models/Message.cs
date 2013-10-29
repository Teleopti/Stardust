using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class Message
    {
        public long MessageId { get; set; }
        public int QueueId { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public System.DateTime ProcessingUntil { get; set; }
        public Nullable<System.DateTime> ExpiresAt { get; set; }
        public bool Processed { get; set; }
        public string Headers { get; set; }
        public byte[] Payload { get; set; }
        public int ProcessedCount { get; set; }
    }
}
