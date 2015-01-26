using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class SeatPlanAddedEvent : EventWithLogOnAndInitiator, ITrackInfo
	{
		public IList<Guid> Teams { get; set; }
		public IList<Guid> Locations { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid TrackId { get; set; }
	}
}