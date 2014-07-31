using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	/// <summary>
	/// Denormalized schedule message
	/// </summary>
	public class ProjectionChangedEventBase : RaptorDomainEvent, ITrackedEvent
	{
		/// <summary>
		/// creates a thingy
		/// </summary>
		public ProjectionChangedEventBase() { IsDefaultScenario = true; }

		/// <summary>
		/// Is this default scenario
		/// </summary>
		public bool IsDefaultScenario { get; set; }
		/// <summary>
		/// Scenario id
		/// </summary>
		public Guid ScenarioId { get; set; }
		/// <summary>
		/// Person id
		/// </summary>
		public Guid PersonId { get; set; }

		/// <summary>
		/// The layers for this shift
		/// </summary>
		public ICollection<ProjectionChangedEventScheduleDay> ScheduleDays { get; set; }

		/// <summary>
		/// Is this the initial load of read models
		/// </summary>
		public bool IsInitialLoad { get; set; }

		public Guid TrackId { get; set; }
	}
}