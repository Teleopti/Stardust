using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public interface INumberOfAgentsInTeamReader
	{
		IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<Guid> teams);
		IDictionary<Guid, int> ForSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds);
	}
}