using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class OptionalColumnValueChangedEvent : EventWithInfrastructureContext
	{
		public Guid PersonId { get; set; }
	}
}