using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class DayOffTemplateChangedEvent : EventWithInfrastructureContext
	{
		public Guid DayOffTemplateId { get; set; }
	}
}
