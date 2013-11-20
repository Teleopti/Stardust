using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ActivityAssignedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public Guid ActivityId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public Guid ScenarioId { get; set; }
	}
}