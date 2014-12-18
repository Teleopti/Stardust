using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class PersonOrganizationData
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
	}
}

