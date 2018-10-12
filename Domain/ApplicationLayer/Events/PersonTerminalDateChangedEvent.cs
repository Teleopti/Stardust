using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonTerminalDateChangedEvent : EventWithInfrastructureContext
	{
		public Guid PersonId { get; set; }
		public string TimeZoneInfoId { get; set; }
		public DateTime? PreviousTerminationDate { get; set; }
		public DateTime? TerminationDate { get; set; }
	}
}