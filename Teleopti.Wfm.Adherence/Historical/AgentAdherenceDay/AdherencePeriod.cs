using System;

namespace Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay
{
	public class AdherencePeriod
	{
		public AdherencePeriod()
		{
		}
		
		public AdherencePeriod(DateTime? startTime, DateTime? endTime)
		{
			StartTime = startTime;
			EndTime = endTime;
		}
		
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	}
}