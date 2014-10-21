namespace Teleopti.Ccc.Web.Areas.Mart.Models
{
	public class FactQueueModel
	{
		public int DateId { get; set; }
		public int IntervalId { get; set; }
		public int LogObjectId { get; set; }
		public int QueueId { get; set; }
		public int OfferedCalls { get; set; }
		public int AnsweredCalls { get; set; }
		public int AnsweredCallsWithinServiceLevel { get; set; }
		public int AbandonedCalls { get; set; }
		public int AbandonedCallsWithinServiceLevel { get; set; }
		public int AbandonedShortCalls { get; set; }
		public int OverflowOutCalls { get; set; }
		public int OverflowInCalls { get; set; }
		public int TalkTime { get; set; }
		public int AfterCallWork { get; set; }
		public int HandleTime { get; set; }
		public int SpeedOfAnswer { get; set; }
		public int TimeToAbandon { get; set; }
		public int LongestDelayInQueueAnswered { get; set; }
		public int LongestDelayInQueueAbandoned { get; set; }
	}
}