using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class AddOverTimeEvent : EventWithInfrastructureContext
	{
		public TimeSpan OvertimeDurationMin { get; set; }
		public TimeSpan OvertimeDurationMax { get; set; }

		public Guid OvertimeType { get; set; } //IMultiplicatorDefinitionSet OvertimeType { get; set; }

		public IList<Guid> Skills { get; set; }
		public Guid? ShiftBagToUse { get; set; }
	}
}