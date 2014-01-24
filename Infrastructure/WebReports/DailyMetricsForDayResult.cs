﻿namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public class DailyMetricsForDayResult
	{
		public int AnsweredCalls { get; set; }
		public int AfterCallWorkTime { get; set; }
		public int TalkTime { get; set; }
		public int HandlingTime { get; set; }
		public int ReadyTimePerScheduledReadyTime { get; set; }
		public int Adherence { get; set; }
	}
}