using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class WorkloadChangedEvent : EventWithInfrastructureContext
	{
		public Guid WorkloadId { get; set; }
	}
}