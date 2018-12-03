using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonEmploymentChangedEvent : EventWithInfrastructureContext
	{
		public Guid PersonId { get; set; }
		public DateOnly FromDate { get; set; }
	}
}
