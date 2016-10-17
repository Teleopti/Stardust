using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
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
		private readonly IUserUiCulture _uiCulture;

		public SiteViewModelBuilder(
			INow now,
			ISiteRepository siteRepository,
			INumberOfAgentsInSiteReader numberOfAgentsInSiteReader,
			ICurrentAuthorization authorization,
			IUserUiCulture uiCulture)
		{
			_now = now;
			_siteRepository = siteRepository;
			_numberOfAgentsInSiteReader = numberOfAgentsInSiteReader;
			_authorization = authorization;
			_uiCulture = uiCulture;
		}

		public IEnumerable<SiteViewModel> Build()
		{
			var sites = allPermittedSites();
			var numberOfAgents = sites.Any() ? _numberOfAgentsInSiteReader.FetchNumberOfAgents(sites.Select(x => x.Id.Value)) : new Dictionary<Guid, int>();
			return sites.Select(site => new SiteViewModel
			{
				Id = site.Id.Value,
				Name = site.Description.Name,
				NumberOfAgents = getNumberOfAgents(numberOfAgents, site.Id.Value),
				OpenHours = openHours(site)
			}).ToList();
		}

		private static IEnumerable<SiteOpenHourViewModel> openHours(ISite site)
		{
			return site.OpenHourCollection.Select(
				openHour =>
					new SiteOpenHourViewModel()
					{
						WeekDay = openHour.WeekDay,
						StartTime = openHour.TimePeriod.StartTime,
						EndTime = openHour.TimePeriod.EndTime,
						IsClosed = openHour.IsClosed
					}).ToList();
		}
		
		public IEnumerable<SiteViewModel> ForSkills(Guid[] skillIds)
		{
			var sites = allPermittedSites();
			var numberOfAgents = sites.Any() ? _numberOfAgentsInSiteReader.ForSkills(sites.Select(x => x.Id.Value), skillIds) : new Dictionary<Guid, int>();
			return sites.Select(site => new SiteViewModel
			{
				Id = site.Id.Value,
				Name = site.Description.Name,
				NumberOfAgents = getNumberOfAgents(numberOfAgents, site.Id.Value)
			}).ToList();
		}

		private IOrderedEnumerable<ISite> allPermittedSites()
		{
			return _siteRepository.LoadAll()
				.Where(
					s =>
						_authorization.Current()
							.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, _now.LocalDateOnly(), s))
				.OrderBy(x => x.Description.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false));
		}

		private static int getNumberOfAgents(IDictionary<Guid, int> agentsInSites, Guid siteId)
		{
			int tempNumberOfAgents;
			return agentsInSites.TryGetValue(siteId, out tempNumberOfAgents) ? tempNumberOfAgents : 0;
		}
	}
}