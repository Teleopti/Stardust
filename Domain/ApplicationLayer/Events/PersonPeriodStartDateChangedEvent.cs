using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	[Serializable]
	public class PersonPeriodStartDateChangedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime OldStartDate { get; set; }
		public DateTime NewStartDate { get; set; }

		public IEnumerable<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
		public IEnumerable<PersonPeriodDetail> PersonPeriodsAfter { get; set; }
	}
}