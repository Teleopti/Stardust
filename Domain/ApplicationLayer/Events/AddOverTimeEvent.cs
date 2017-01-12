using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class AddOverTimeEvent : EventWithInfrastructureContext
	{
		public DateTimePeriod Period { get; set; }
		public IList<Guid> Skills { get; set; }
	}
}