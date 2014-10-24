namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Interface for queue statistics
	/// </summary>
	/// <remarks>
	/// Created by: marias
	/// Created date: 2014-10-24
	/// </remarks>
	public interface IQueueStatsModel
	{
		string DateAndTimeString { get; set; }
		string LogObjectName { get; set; }
		string QueueId { get; set; }
		string QueueName { get; set; }
		string NhibName { get; set; }
		int OfferedCalls { get; set; }
		int AnsweredCalls { get; set; }
		int AnsweredCallsWithinServiceLevel { get; set; }
		int AbandonedCalls { get; set; }
		int AbandonedCallsWithinServiceLevel { get; set; }
		int AbandonedShortCalls { get; set; }
		int OverflowOutCalls { get; set; }
		int OverflowInCalls { get; set; }
		int TalkTime { get; set; }
		int AfterCallWork { get; set; }
		int SpeedOfAnswer { get; set; }
		int TimeToAbandon { get; set; }
		int LongestDelayInQueueAnswered { get; set; }
		int LongestDelayInQueueAbandoned { get; set; }
	}
}