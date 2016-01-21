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
	}
}

