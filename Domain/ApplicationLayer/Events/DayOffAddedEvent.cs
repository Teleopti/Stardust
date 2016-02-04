using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class DayOffAddedEvent : EventWithInfrastructureContext
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public Guid ScenarioId { get; set; }
	}
}