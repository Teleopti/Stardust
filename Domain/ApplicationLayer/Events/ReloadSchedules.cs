using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ReloadSchedules : EventWithInfrastructureContext
	{
		public ICollection<DateTime> Dates { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid PersonId { get; set; }
	}
}