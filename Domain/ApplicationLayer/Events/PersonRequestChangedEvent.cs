using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonRequestCreatedEvent : PersonRequestChangedBase
	{
	}
	public class PersonRequestDeletedEvent : PersonRequestChangedBase
	{
	}
	public class PersonRequestChangedEvent : PersonRequestChangedBase
	{
	}

	public class PersonRequestChangedBase : EventWithInfrastructureContext
	{
		public Guid PersonRequestId { get; set; }
	}
}