using System;

namespace Teleopti.Analytics.Stats.TestApplication
{
	internal class QueueDataParameters
	{
		public int IntervalLength { get; set; }
		public int AmountOfQueues { get; set; }
		public int AmountOfDays { get; set; }
		public DateTime StartDate { get; set; }
		public string NhibDataSourcename { get; set; }
		public int QueueDataSourceId { get; set; }
		public bool UseLatency { get; set; }
	}
}