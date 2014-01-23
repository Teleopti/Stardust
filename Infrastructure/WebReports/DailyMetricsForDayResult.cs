namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public class DailyMetricsForDayResult
	{
		public DailyMetricsForDayResult()
		{
			DataAvailable = true;
		}

		public int AnsweredCalls { get; set; }
		public int AfterCallWorkTime { get; set; }
		public int TalkTime { get; set; }
		public int HandlingTime { get; set; }
		public int ReadyTimePerScheduledReadyTime { get; set; }
		public bool DataAvailable { get; set; }
	}
}