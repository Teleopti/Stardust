namespace Teleopti.Ccc.Intraday.TestApplication
{
	public class QueueInterval
	{
		public int QueueId { get; set; }
		public int DateId { get; set; }
		public int IntervalId { get; set; }
		public decimal OfferedCalls { get; set; }
		public decimal HandleTime { get; set; }
	    public int DatasourceId { get; set; }
	}
}