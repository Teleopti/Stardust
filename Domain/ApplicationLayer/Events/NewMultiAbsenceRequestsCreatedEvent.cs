using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class NewMultiAbsenceRequestsCreatedEvent : EventWithInfrastructureContext
	{
		public List<Guid> PersonRequestIds { get; set; }
		public DateTime Sent { get; set; }
	}
}
