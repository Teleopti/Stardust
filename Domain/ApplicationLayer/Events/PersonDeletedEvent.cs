using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	[Serializable]
	public class PersonDeletedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }

		public IEnumerable<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
	}
}