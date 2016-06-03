using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class SiteViewModelBuilder
	{
		private readonly IPersonalAvailableDataProvider _personalAvailableDataProvider;
		private readonly INow _now;
		private readonly ISiteRepository _siteRepository;
		private readonly INumberOfAgentsInSiteReader _numberOfAgentsInSiteReader;

		public SiteViewModelBuilder(
			IPersonalAvailableDataProvider 
			personalAvailableDataProvider, 
			INow now, 
			ISiteRepository siteRepository, 
			INumberOfAgentsInSiteReader numberOfAgentsInSiteReader)
		{
			_personalAvailableDataProvider = personalAvailableDataProvider;
			_now = now;
			_siteRepository = siteRepository;
			_numberOfAgentsInSiteReader = numberOfAgentsInSiteReader;
		}

		public IEnumerable<SiteViewModel> Build()
		{
			var sites = _personalAvailableDataProvider != null
				? _personalAvailableDataProvider.AvailableSites(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview,
					_now.LocalDateOnly()).ToArray()
				: _siteRepository.LoadAll();

			IDictionary<Guid, int> numberOfAgents = new Dictionary<Guid, int>();
			if (sites.Any())
				numberOfAgents = _numberOfAgentsInSiteReader.FetchNumberOfAgents(sites);

			return sites.Select(site =>
			{
				var valueOrDefault = site.Id.GetValueOrDefault();
				return new SiteViewModel
				{
					Id = valueOrDefault,
					Name = site.Description.Name,
					NumberOfAgents = numberOfAgents[valueOrDefault]
				};
			});
		}
	}
}