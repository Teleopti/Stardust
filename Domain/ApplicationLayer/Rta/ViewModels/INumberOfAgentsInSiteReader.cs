using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public interface INumberOfAgentsInSiteReader
	{
		IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<Guid> siteIds);
		IDictionary<Guid, int> ForSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds);
	}
}
