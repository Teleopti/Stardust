using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonDeletedEvent : EventWithLogOnAndInitiator
	{
		public Guid PersonId { get; set; }

		public ICollection<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
	}
}