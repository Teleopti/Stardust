using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ActivityChangedEvent : EventWithInfrastructureContext
	{
		public Guid ActivityId { get; set; }
		public string Property { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
	}
}