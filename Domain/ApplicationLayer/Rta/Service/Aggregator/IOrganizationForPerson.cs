using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator
{
	public interface IOrganizationForPerson
	{
		PersonOrganizationData GetOrganization(Guid personId, string tenant);
	}
}