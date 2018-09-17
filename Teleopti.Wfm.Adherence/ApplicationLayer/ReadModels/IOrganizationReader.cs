using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels
{
	public interface IOrganizationReader
	{
		IEnumerable<OrganizationSiteModel> Read();
		IEnumerable<OrganizationSiteModel> Read(IEnumerable<Guid> skillIds);
	}
}