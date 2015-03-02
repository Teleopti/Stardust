using System;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class AverageStatistics
	{
		public double AverageTasks { get; set; }
		public TimeSpan TalkTime { get; set; }
		public TimeSpan AfterTalkTime { get; set; }
	}
}