using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.Monitor.Infrastructure
{
	public interface IOrganizationReader
	{
		IEnumerable<OrganizationSiteModel> Read();
		IEnumerable<OrganizationSiteModel> Read(IEnumerable<Guid> skillIds);
	}
}