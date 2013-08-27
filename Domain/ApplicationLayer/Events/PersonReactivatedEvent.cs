using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	[Serializable]
	public class PersonReactivatedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime PreviousTerminationDate { get; set; }

		public IEnumerable<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
		public IEnumerable<PersonPeriodDetail> PersonPeriodsAfter { get; set; }
	}
}