using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ActivityChangedEvent : EventWithInfrastructureContext
	{
		public Guid ActivityId { get; set; }
	}
}