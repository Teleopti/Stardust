using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class NewAbsenceReportCreatedEvent : EventWithInfrastructureContext
	{
		
		public Guid AbsenceId { get; set; }
		public DateTime RequestedDate { get; set; }
		public Guid Identity
		{
			get { return AbsenceId; }
		}

		public Guid PersonId { get; set; }
	}
}