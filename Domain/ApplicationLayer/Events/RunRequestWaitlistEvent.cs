using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class RunRequestWaitlistEvent : EventWithInfrastructureContext
	{
		public RunRequestWaitlistEvent()
		{
			Identity = Guid.NewGuid();
		}

		public Guid Identity { get; private set; }

		public DateTimePeriod Period { get; set; }
	}
}