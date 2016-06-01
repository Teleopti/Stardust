using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class RequestChangedEvent : EventWithInfrastructureContext
	{
		public Guid RequestId { get; set; }
	}
}