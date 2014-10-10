using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class PersonOutOfAdherenceEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public DateTime Timestamp { get; set; }

	}
}