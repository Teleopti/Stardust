using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aggregator
{
	public interface IOrganizationForPerson
	{
		PersonOrganizationData GetOrganization(Guid personId);
	}
}