using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class TeamOutOfAdherenceReadModel
	{
		public TeamOutOfAdherenceReadModel()
		{
			PersonIds = "";
		}

		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
		public int Count { get; set; }
		public string PersonIds { get; set; }

	}
}