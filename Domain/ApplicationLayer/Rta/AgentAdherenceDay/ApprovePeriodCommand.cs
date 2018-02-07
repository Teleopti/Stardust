using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public class ApprovePeriodCommand
	{
		public Guid PersonId;
		public string StartDateTime;
		public string EndDateTime;
	}
}