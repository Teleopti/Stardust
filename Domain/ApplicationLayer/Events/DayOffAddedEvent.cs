using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class DayOffAddedEvent : EventWithInfrastructureContext, ICommandIdentifier
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }		
		public Guid ScenarioId { get; set; }
		public Guid CommandId { get; set; }
	}
}