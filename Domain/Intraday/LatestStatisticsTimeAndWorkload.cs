using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class LatestStatisticsTimeAndWorkload
	{
		public int? LatestStatisticsStartTime { get; set; }
		public IList<SkillIntervalCalls> ActualWorkloadInSecondsPerSkill { get; set; }
	}

	public class SkillIntervalCalls
	{
		public Guid SkillId { get; set; }
		public double Calls { get; set; }
		public DateTime StartTime { get; set; }
	}
}