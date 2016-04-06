using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class NewAbsenceRequestCreated : EventWithInfrastructureContext
	{
		public Guid PersonRequestId { get; set; }

		public Guid Identity
		{
			get { return PersonRequestId; }
		}
	}
}
