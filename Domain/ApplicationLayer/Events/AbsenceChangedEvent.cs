using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class AbsenceChangedEvent : EventWithInfrastructureContext
	{
		public Guid AbsenceId { get; set; }
	}
}