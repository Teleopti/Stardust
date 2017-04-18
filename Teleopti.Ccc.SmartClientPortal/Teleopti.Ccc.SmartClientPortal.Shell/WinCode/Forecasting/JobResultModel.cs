using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting
{
    public class JobResultModel
    {
        public Guid? JobId { get; set; }
        public string JobCategory { get; set; }
        public virtual string Owner { get; set; }
        public virtual string Timestamp { get; set; }
        public virtual string Status { get; set; }
    }
}
