using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class AbsenceTypeChangeEvent : EventWithInfrastructureContext
	{
		public Guid AbsenceId { get; set; }
	}
}