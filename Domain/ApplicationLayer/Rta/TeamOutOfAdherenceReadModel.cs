using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class TeamOutOfAdherenceReadModel
	{
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
		public int Count { get; set; }
		public IEnumerable<TeamOutOfAdherenceReadModelState> State { get; set; }
	}

	public class TeamOutOfAdherenceReadModelState
	{
		public int OutOfAdherence { get; set; }
		public Guid PersonId { get; set; }
	}
}