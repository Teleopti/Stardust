using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class AvailabilityChangedEvent : EventWithInfrastructureContext
	{
		public DateOnly Date { get; set; }
		public Guid PersonId { get; set; }
		public Guid AvailabilityId { get; set; }
	}
}