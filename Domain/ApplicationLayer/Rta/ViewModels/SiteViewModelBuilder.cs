using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class SiteViewModelBuilder
	{
		private readonly INow _now;
		private readonly ISiteRepository _siteRepository;
		private readonly INumberOfAgentsInSiteReader _numberOfAgentsInSiteReader;
		private readonly ICurrentAuthorization _authorization;
		private readonly IUserCulture _culture;

		public SiteViewModelBuilder(
			INow now, 
			ISiteRepository siteRepository, 
			INumberOfAgentsInSiteReader numberOfAgentsInSiteReader,
			ICurrentAuthorization authorization,
			IUserCulture culture)
		{
			_now = now;
			_siteRepository = siteRepository;
			_numberOfAgentsInSiteReader = numberOfAgentsInSiteReader;
			_authorization = authorization;
			_culture = culture;
		}

		public IEnumerable<SiteViewModel> Build()
		{
			var sites = _siteRepository.LoadAll()
				.Where(s => _authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, _now.LocalDateOnly(), s))
				.OrderBy(x => x.Description.Name, StringComparer.Create(_culture.GetCulture(), false));

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