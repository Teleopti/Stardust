﻿namespace Teleopti.Ccc.Intraday.TestCommon
{
	public class QueueInterval
	{
		public int QueueId { get; set; }
		public int DateId { get; set; }
		public int IntervalId { get; set; }
		public decimal OfferedCalls { get; set; }
		public decimal HandleTime { get; set; }
	    public int DatasourceId { get; set; }
		public decimal TalkTime { get; set; }
		public decimal Acw { get; set; }
	    public decimal AnsweredCalls { get; set; }
	    public decimal AbandonedCalls { get; set; }
	    public decimal SpeedOfAnswer { get; set; }
	    public decimal AnsweredCallsWithinSL { get; set; }
	}
}