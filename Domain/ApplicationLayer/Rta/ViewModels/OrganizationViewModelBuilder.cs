using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class OrganizationViewModelBuilder
	{
		private readonly INow _now;
		private readonly ISiteRepository _siteRepository;
		private readonly INumberOfAgentsInSiteReader _numberOfAgentsInSiteReader;
		private readonly INumberOfAgentsInTeamReader _numberOfAgentsInTeamReader;
		private readonly ICurrentAuthorization _authorization;
		private readonly IUserUiCulture _uiCulture;

		public OrganizationViewModelBuilder(
			INow now,
			ISiteRepository siteRepository,
			INumberOfAgentsInSiteReader numberOfAgentsInSiteReader,
			INumberOfAgentsInTeamReader numberOfAgentsInTeamReader,
			ICurrentAuthorization authorization,
			IUserUiCulture uiCulture)
		{
			_now = now;
			_siteRepository = siteRepository;
			_numberOfAgentsInSiteReader = numberOfAgentsInSiteReader;
			_numberOfAgentsInTeamReader = numberOfAgentsInTeamReader;
			_authorization = authorization;
			_uiCulture = uiCulture;
		}

		private static IEnumerable<SiteOpenHourViewModel> openHours(ISite site)
		{
			return site.OpenHourCollection.Select(openHour => new SiteOpenHourViewModel
			{
				WeekDay = openHour.WeekDay,
				StartTime = openHour.TimePeriod.StartTime,
				EndTime = openHour.TimePeriod.EndTime,
				IsClosed = openHour.IsClosed
			}).ToList();
		}

		private IOrderedEnumerable<ISite> allPermittedSites()
		{
			return _siteRepository.LoadAll()
				.Where(s => _authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview,
					_now.LocalDateOnly(), s))
				.OrderBy(x => x.Description.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false));
		}

		public IEnumerable<OrganizationSiteViewModel> Build()
		{
			var sites = allPermittedSites();
			var org =
				from site in sites
				where site.TeamCollection.Count > 0
				select new OrganizationSiteViewModel
				{
					Id = site.Id.Value,
					Name = site.Description.Name,
					OpenHours = openHours(site),
					Teams = site.TeamCollection
						.Select(
							team => new OrganizationTeamViewModel
							{
								Id = team.Id.Value,
								Name = team.Description.Name
							})
						.OrderBy(t => t.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false))
						.ToArray()
				};
			return org.ToArray();
		}

		public IEnumerable<OrganizationSiteViewModel> BuildForSkills(Guid[] skillIds)
		{
			var sites = allPermittedSites();
			// Dont need to check number of agents for site?
			var numberOfAgents = sites.Any()
				? _numberOfAgentsInSiteReader.Read(sites.Select(x => x.Id.Value), skillIds)
				: new Dictionary<Guid, int>();
			var org = from num in numberOfAgents
				from site in sites
				let teamIdsBySkill = site.TeamCollection.Any()
					? _numberOfAgentsInTeamReader.ForSkills(site.TeamCollection.Select(t => t.Id.Value), skillIds)
					: new Dictionary<Guid, int>()
				where num.Key == site.Id.Value && num.Value > 0 && site.TeamCollection.Count > 0
				select new OrganizationSiteViewModel
				{
					Id = site.Id.Value,
					Name = site.Description.Name,
					Teams = site.TeamCollection
						.Where(team => teamIdsBySkill.ContainsKey(team.Id.Value))
						.Select(
							team => new OrganizationTeamViewModel
							{
								Id = team.Id.Value,
								Name = team.Description.Name
							}).ToArray()
				};
			return org.ToArray();
		}
	}

	public class OrganizationSiteViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<SiteOpenHourViewModel> OpenHours;
		public IEnumerable<OrganizationTeamViewModel> Teams;
	}

	public class OrganizationTeamViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}