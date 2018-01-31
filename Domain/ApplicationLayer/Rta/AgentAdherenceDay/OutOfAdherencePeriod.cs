using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public class OutOfAdherencePeriod
	{
		public DateTime StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	}
}