using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class RunRequestWaitlistEvent : EventWithInfrastructureContext
	{
		public RunRequestWaitlistEvent(DateTimePeriod period)
		{
			Identity = Guid.NewGuid();
			Period = period;
		}

		public Guid Identity { get; private set; }

		public DateTimePeriod Period { get; private set; }
	}
}