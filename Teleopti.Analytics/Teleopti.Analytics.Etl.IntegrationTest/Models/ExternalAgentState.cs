using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class ExternalAgentState
    {
        public long Id { get; set; }
        public string LogOn { get; set; }
        public string StateCode { get; set; }
        public long TimeInState { get; set; }
        public System.DateTime TimestampValue { get; set; }
        public Nullable<System.Guid> PlatformTypeId { get; set; }
        public Nullable<int> DataSourceId { get; set; }
        public Nullable<System.DateTime> BatchId { get; set; }
        public bool IsSnapshot { get; set; }
    }
}
