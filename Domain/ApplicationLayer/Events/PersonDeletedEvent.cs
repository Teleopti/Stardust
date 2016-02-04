using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonDeletedEvent : EventWithInfrastructureContext
	{
		public Guid PersonId { get; set; }
	}
}