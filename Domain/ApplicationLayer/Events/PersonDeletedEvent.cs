using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonDeletedEvent : EventWithLogOnAndInitiator
	{
		public Guid PersonId { get; set; }
	}
}