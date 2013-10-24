using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class Queues1
    {
        public string QueueName { get; set; }
        public int QueueId { get; set; }
        public Nullable<int> ParentQueueId { get; set; }
        public string Endpoint { get; set; }
    }
}
