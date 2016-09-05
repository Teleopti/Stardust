using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class QueueStatisticsInterval
	{
		public DateTime StartTime { get; set; }
		public double Calls { get; set; }
		public double HandleTime { get; set; }
	}
}