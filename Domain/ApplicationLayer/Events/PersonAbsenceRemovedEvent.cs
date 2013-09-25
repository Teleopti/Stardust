using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonAbsenceRemovedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public Guid ScenarioId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}
}