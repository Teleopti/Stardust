using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonPeriodStartDateChangedEvent : EventWithLogOnAndInitiator
	{
		public Guid PersonId { get; set; }
		public DateTime OldStartDate { get; set; }
		public DateTime NewStartDate { get; set; }

		public ICollection<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
		public ICollection<PersonPeriodDetail> PersonPeriodsAfter { get; set; }
	}
}