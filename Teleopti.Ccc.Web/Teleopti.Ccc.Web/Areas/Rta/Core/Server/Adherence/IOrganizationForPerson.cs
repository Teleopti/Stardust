using System;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public interface IOrganizationForPerson
	{
		PersonOrganizationData GetOrganization(Guid personId);
	}
}