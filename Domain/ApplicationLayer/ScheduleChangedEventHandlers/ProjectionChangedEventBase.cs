using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ProjectionChangedEventBase : EventWithInfrastructureContext, ICommandIdentifier
	{
		public ProjectionChangedEventBase() { IsDefaultScenario = true; }
		public bool IsDefaultScenario { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid PersonId { get; set; }
		public ICollection<ProjectionChangedEventScheduleDay> ScheduleDays { get; set; }
		public bool IsInitialLoad { get; set; }
		public Guid CommandId { get; set; }
		public int RetriesCount { get; set; }
		public DateTime ScheduleLoadTimestamp { get; set; }
	}
}