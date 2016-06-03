using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeNumberOfAgentsInSiteReader : INumberOfAgentsInSiteReader
	{
		private readonly ISiteRepository _siteRepository;

		public FakeNumberOfAgentsInSiteReader(ISiteRepository siteRepository)
		{
			_siteRepository = siteRepository;
		}

		public IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<ISite> sites)
		{
			// simple stub implementation for now
			return _siteRepository.LoadAll()
				.ToDictionary(s => s.Id.GetValueOrDefault(), s => 0);
		}
	}
}
