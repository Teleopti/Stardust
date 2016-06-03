using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeNumberOfAgentsInTeamReader : INumberOfAgentsInTeamReader
	{
		public IDictionary<Guid, int> FetchNumberOfAgents(ITeam[] teams)
		{
			return new Dictionary<Guid, int> { { Guid.Empty, 0 } };
		}
	}
}