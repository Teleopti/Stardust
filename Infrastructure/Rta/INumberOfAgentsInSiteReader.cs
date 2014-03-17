using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public interface INumberOfAgentsInSiteReader
	{
		IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<ISite> sites);
	}
}
