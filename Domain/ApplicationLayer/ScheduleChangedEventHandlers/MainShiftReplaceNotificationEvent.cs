using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class MainShiftReplaceNotificationEvent : EventWithInfrastructureContext 
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid PersonId { get; set; }
		public Guid CommandId { get; set; }
	}
}