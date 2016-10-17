using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public interface INumberOfAgentsInSiteReader
	{
		IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<Guid> sites);
		IDictionary<Guid, int> ForSkills(IEnumerable<Guid> sites, IEnumerable<Guid> skillIds);
	}
}
