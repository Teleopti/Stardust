using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class NewShiftTradeRequestCreatedEvent : EventWithInfrastructureContext
	{
		public Guid PersonRequestId { get; set; }
	}
}