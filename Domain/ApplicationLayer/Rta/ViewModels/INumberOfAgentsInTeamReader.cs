using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public interface INumberOfAgentsInTeamReader
	{
		IDictionary<Guid, int> FetchNumberOfAgents(ITeam[] teams);
	}
}