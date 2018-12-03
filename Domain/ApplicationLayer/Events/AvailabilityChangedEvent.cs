using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class AvailabilityChangedEvent : EventWithInfrastructureContext
	{
		public List<DateOnly> Dates { get; set; }
		public Guid PersonId { get; set; }
	}
}