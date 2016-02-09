using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class TeamOutOfAdherenceReadModel
	{
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
		public int Version { get; set; }
		public int Count { get; set; }
		public IEnumerable<TeamOutOfAdherenceReadModelState> State { get; set; }
	}

	public class TeamOutOfAdherenceReadModelState
	{
		public Guid PersonId { get; set; }
		public bool OutOfAdherence { get; set; }
		public bool Deleted { get; set; }
		public bool Moved { get; set; }
		public DateTime Time { get; set; }
	}
}