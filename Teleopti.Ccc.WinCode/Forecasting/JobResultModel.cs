namespace Teleopti.Ccc.WinCode.Forecasting
{
    public class JobResultModel
    {
        public string JobCategory { get; set; }
        public virtual string Owner { get; set; }
        public virtual string Timestamp { get; set; }
        public virtual string Status { get; set; }
    }
}
