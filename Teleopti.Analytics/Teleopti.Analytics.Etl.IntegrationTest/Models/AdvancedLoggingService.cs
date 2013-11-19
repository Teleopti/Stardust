using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class AdvancedLoggingService
    {
        public System.DateTime LogDate { get; set; }
        public string Message { get; set; }
        public string BU { get; set; }
        public Nullable<System.Guid> BUId { get; set; }
        public string DataSource { get; set; }
        public string WindowsIdentity { get; set; }
        public string HostIP { get; set; }
        public string BlockOptions { get; set; }
        public string TeamOptions { get; set; }
        public string GeneralOptions { get; set; }
        public Nullable<int> SkillDays { get; set; }
        public Nullable<int> Agents { get; set; }
        public Nullable<int> ExecutionTime { get; set; }
    }
}
