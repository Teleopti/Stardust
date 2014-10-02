using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public interface IPersonOrganizationProvider
	{
		IDictionary<Guid, PersonOrganizationData> LoadAll();
	}
}