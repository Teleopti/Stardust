using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeNumberOfAgentsInTeamReader : INumberOfAgentsInTeamReader
	{
		public IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<Guid> teams)
		{
			return new Dictionary<Guid, int> { { Guid.Empty, 0 } };
		}
	}
}