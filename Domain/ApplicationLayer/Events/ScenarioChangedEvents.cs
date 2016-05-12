using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ScenarioChangeEvent : EventWithInfrastructureContext
	{
		public Guid ScenarioId { get; set; }
	}

	public class ScenarioDeleteEvent : EventWithInfrastructureContext
	{
		public Guid ScenarioId { get; set; }
	}
}
