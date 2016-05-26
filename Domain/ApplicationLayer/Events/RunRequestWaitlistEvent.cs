using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class RunRequestWaitlistEvent : EventWithInfrastructureContext
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	    public Guid CommandId { get; set; }
    }
}