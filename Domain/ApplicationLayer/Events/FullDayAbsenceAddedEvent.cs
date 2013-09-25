using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class FullDayAbsenceAddedEvent : RaptorDomainEvent
	{
		public Guid AbsenceId { get; set; }
		public Guid PersonId { get; set; }
		public Guid ScenarioId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}
}