using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class PersonOrganizationData
	{
		public string Tenant { get; set; }
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
	}
}

