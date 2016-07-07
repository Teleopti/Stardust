using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeNumberOfAgentsInSiteReader : INumberOfAgentsInSiteReader
	{
		public IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<ISite> sites)
		{
			return new Dictionary<Guid, int> {{Guid.Empty, 0}};
		}
	}
}
