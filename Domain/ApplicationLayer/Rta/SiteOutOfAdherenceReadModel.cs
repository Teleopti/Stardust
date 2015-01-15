using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class SiteOutOfAdherenceReadModel
	{
		public SiteOutOfAdherenceReadModel()
		{
			PersonIds = "";
		}

		public int Count { get; set; }
		public Guid SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public string PersonIds { get; set; }

	}
}