using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ActivityDeleteEvent : EventWithInfrastructureContext
	{
		public Guid ActivityId { get; set; }
	}
}