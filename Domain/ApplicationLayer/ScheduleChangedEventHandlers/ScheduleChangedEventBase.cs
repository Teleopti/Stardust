using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public abstract class ScheduleChangedEventBase : RaptorDomainEvent
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid PersonId { get; set; }
		public bool SkipDelete { get; set; }
	}
}