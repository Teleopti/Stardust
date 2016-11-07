using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ArchiveScheduleEvent : EventWithInfrastructureContext
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid FromScenario { get; set; }
		public Guid ToScenario { get; set; }
		public Guid PersonId { get; set; }
		public Guid TrackingId { get; set; }
	}
}