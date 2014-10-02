using System;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public interface IOrganizationForPerson
	{
		PersonOrganizationData GetOrganization(Guid personId);
	}
}