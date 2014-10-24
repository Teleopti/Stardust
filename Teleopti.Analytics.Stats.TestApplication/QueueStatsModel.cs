using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Stats.TestApplication
{
	public class QueueStatsModel : IQueueStatsModel
	{
		public string NhibName { get; set; }
		public string LogObjectName { get; set; }

		public string DateAndTimeString { get; set; }
		public string QueueId { get; set; }
		public string QueueName { get; set; }
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
		public int SpeedOfAnswer { get; set; }
		public int TimeToAbandon { get; set; }
		public int LongestDelayInQueueAnswered { get; set; }
		public int LongestDelayInQueueAbandoned { get; set; }
	}
}