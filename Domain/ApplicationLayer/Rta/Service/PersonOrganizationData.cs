using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class PersonOrganizationData
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
		public string TimeZone { get; set; }
		public DateTime EndDate { get; set; }

		// for logging
		public override string ToString()
		{
			return string.Format(
				"PersonId: {0}, BusinessUnitId: {1}, TeamId: {2}, SiteId: {3}",
				PersonId, BusinessUnitId, TeamId, SiteId);
		}
	}
}

