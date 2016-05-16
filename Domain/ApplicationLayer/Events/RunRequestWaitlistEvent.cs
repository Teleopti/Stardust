using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class RunRequestWaitlistEvent : EventWithInfrastructureContext
	{
		public RunRequestWaitlistEvent()
		{
			Id = Guid.NewGuid();
		}

		public Guid Id { get; private set; }

		public DateTimePeriod Period { get; set; }
	}
}