using System;

namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public class QueueMetricsForDayResult
	{
        public string Person { get; set; }
		public string Queue { get; set; }
        public int AnsweredCalls { get; set; }
        public TimeSpan AverageAfterCallWorkTime { get; set; }
        public TimeSpan AverageTalkTime { get; set; }
        public TimeSpan AverageHandlingTime { get; set; }
	}
}