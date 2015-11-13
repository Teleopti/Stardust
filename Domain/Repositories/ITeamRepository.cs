using System;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Repositories
{

	/// <summary>
	/// Defines the functionality of a .
	/// </summary>
	public interface ITeamRepository : IRepository<ITeam>
	{

		ICollection<ITeam> FindAllTeamByDescription();

		ICollection<ITeam> FindTeamByDescriptionName(string name);

		ICollection<ITeam> FindTeams (IEnumerable<Guid> teamId);


		IEnumerable<ITeam> FindTeamsStartWith(string searchString, int maxHits);
	}

}
