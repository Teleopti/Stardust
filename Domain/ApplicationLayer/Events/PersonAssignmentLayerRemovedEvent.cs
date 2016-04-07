using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonAssignmentLayerRemovedEvent : EventWithInfrastructureContext, ICommandIdentifier
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }	
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid CommandId { get; set; }
	}
}
