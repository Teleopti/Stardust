using System;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public class JobResultDetailModel
    {
        public Guid? JobId { get; set; }
        public string Message { get; set; }
        public virtual string ExceptionStackTrace { get; set; }
        public virtual string ExceptionMessage { get; set; }
        public virtual string InnerExceptionStackTrace { get; set; }
        public virtual string InnerExceptionMessage { get; set; }
        public virtual DateTime Timestamp { get; set; }
    }
}
