using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class SiteOutOfAdherenceReadModel
	{
		public Guid SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public int Version { get; set; }
		public int Count { get; set; }
		public IEnumerable<SiteOutOfAdherenceReadModelState> State { get; set; }
	}

	public class SiteOutOfAdherenceReadModelState
	{
		public Guid PersonId { get; set; }
		public bool OutOfAdherence { get; set; }
		public bool Moved { get; set; }
		public bool Deleted { get; set; }
		public DateTime Time { get; set; }
	}
}