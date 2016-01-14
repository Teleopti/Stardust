using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonTerminalDateChangedEvent : EventWithLogOnAndInitiator
	{
		public Guid PersonId { get; set; }
		public DateTime? PreviousTerminationDate { get; set; }
		public DateTime? TerminationDate { get; set; }

		public ICollection<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
		public ICollection<PersonPeriodDetail> PersonPeriodsAfter { get; set; }
	}
}