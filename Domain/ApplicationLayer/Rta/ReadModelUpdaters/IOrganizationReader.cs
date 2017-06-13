using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IOrganizationReader
	{
		IEnumerable<OrganizationModel> Read();
		IEnumerable<OrganizationModel> Read(IEnumerable<Guid> skillIds);
	}
}