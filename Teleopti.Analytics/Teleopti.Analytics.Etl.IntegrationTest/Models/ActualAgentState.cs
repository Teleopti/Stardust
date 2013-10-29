using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class ActualAgentState
    {
        public System.Guid PersonId { get; set; }
        public string StateCode { get; set; }
        public System.Guid PlatformTypeId { get; set; }
        public string State { get; set; }
        public System.Guid StateId { get; set; }
        public string Scheduled { get; set; }
        public System.Guid ScheduledId { get; set; }
        public System.DateTime StateStart { get; set; }
        public string ScheduledNext { get; set; }
        public System.Guid ScheduledNextId { get; set; }
        public System.DateTime NextStart { get; set; }
        public string AlarmName { get; set; }
        public System.Guid AlarmId { get; set; }
        public int Color { get; set; }
        public System.DateTime AlarmStart { get; set; }
        public double StaffingEffect { get; set; }
        public System.DateTime ReceivedTime { get; set; }
        public Nullable<System.DateTime> BatchId { get; set; }
        public string OriginalDataSourceId { get; set; }
    }
}
