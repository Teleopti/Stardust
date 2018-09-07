using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels
{
	public interface IOrganizationReader
	{
		IEnumerable<OrganizationSiteModel> Read();
		IEnumerable<OrganizationSiteModel> Read(IEnumerable<Guid> skillIds);
	}
}