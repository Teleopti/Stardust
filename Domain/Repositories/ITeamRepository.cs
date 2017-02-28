using System;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ITeamRepository : IRepository<ITeam>
	{
		ICollection<ITeam> FindAllTeamByDescription();
		ICollection<ITeam> FindTeamByDescriptionName(string name);
		ICollection<ITeam> FindTeams (IEnumerable<Guid> teamId);
		IEnumerable<ITeam> FindTeamsContain(string searchString, int maxHits);
		IEnumerable<ITeam> FindTeamsForSite(Guid siteId);
	}

}
