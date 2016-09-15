using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class LatestStatisticsTimeAndWorkload
	{
		public int? ActualworkloadInSeconds { get; set; }
		public int? LatestStatisticsIntervalId { get; set; }
		public IList<SkillWorkload> ActualWorkloadInSecondsPerSkill { get; set; }
	}

	public class SkillWorkload
	{
		public Guid SkillId { get; set; }
		public double WorkloadInSeconds { get; set; }
		public DateTime StartTime { get; set; }
	}

}