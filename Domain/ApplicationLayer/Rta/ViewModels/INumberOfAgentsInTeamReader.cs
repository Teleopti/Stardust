using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public interface INumberOfAgentsInTeamReader
	{
		IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<Guid> teams);
	}
}