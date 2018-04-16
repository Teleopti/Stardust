using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class AcceptShiftTradeEvent : EventWithInfrastructureContext
	{
		public Guid AcceptingPersonId { get; set; }

		public Guid PersonRequestId { get; set; }

		public string Message { get; set; }

		public bool UseSiteOpenHoursRule { get; set; }
	}
}