using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonPeriodRemovedEvent : EventWithLogOnAndInitiator
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }

		public ICollection<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
		public ICollection<PersonPeriodDetail> PersonPeriodsAfter { get; set; }
	}
}