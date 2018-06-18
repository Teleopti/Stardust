using System;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public class SkillIntervalStatistics
	{
		public Guid SkillId { get; set; }
		public Guid WorkloadId { get; set; }
		public double Calls { get; set; }
		public DateTime StartTime { get; set; }
		public double AverageHandleTime { get; set; }
		public int AnsweredCalls { get; set; }
		public double HandleTime { get; set; }
	}
}