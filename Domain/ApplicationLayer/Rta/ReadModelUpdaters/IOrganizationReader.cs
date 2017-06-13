using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IOrganizationReader
	{
		IEnumerable<OrganizationSiteModel> Read();
		IEnumerable<OrganizationSiteModel> Read(IEnumerable<Guid> skillIds);
	}
}