using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Ajax.Utilities;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core
{
	public class TeamsProvider : ITeamsProvider
	{
		private readonly ISiteRepository _siteRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;

		public TeamsProvider(ISiteRepository siteRepository,
			ICurrentBusinessUnit currentBusinessUnit,
			IPermissionProvider permissionProvider,
			ILoggedOnUser loggedOnUser,
			IPersonSelectorReadOnlyRepository personSelectorReadOnlyRepository)
		{
			_siteRepository = siteRepository;
			_currentBusinessUnit = currentBusinessUnit;
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
			_personSelectorReadOnlyRepository = personSelectorReadOnlyRepository;
		}

		public IEnumerable<TeamViewModel> Get(string siteId)
		{
			var site = _siteRepository.Get(new Guid(siteId));
			return getTeamsForSite(site);
		}

		private IEnumerable<TeamViewModel> getTeamsForSite(ISite site)
		{
			return site.SortedTeamCollection
				.Where(team => team.IsChoosable)
				.Select(team => new TeamViewModel
				{
					Id = team.Id.Value.ToString(),
					Name = team.Description.Name

				});
		}

		public BusinessUnitWithSitesViewModel GetTeamHierarchy()
		{
			var sites = _siteRepository.LoadAll().OrderBy(site => site.Description.Name);
			var siteViewModels = new List<SiteViewModelWithTeams>();

			foreach (var site in sites)
			{
				var siteViewModel = new SiteViewModelWithTeams
				{
					Id = site.Id.ToString(),
					Name = site.Description.Name
				};

				siteViewModels.Add(siteViewModel);
				var teamViewModels = getTeamsForSite(site);
				siteViewModel.Children.AddRange(teamViewModels);
			}

			return new BusinessUnitWithSitesViewModel
			{
				Id = _currentBusinessUnit.Current().Id ?? Guid.Empty,
				Name = _currentBusinessUnit.Current().Name,
				Children = siteViewModels
			};

		}
		
		public BusinessUnitWithSitesViewModel GetPermittedTeamHierachy(DateOnly date, string permission)
		{
			var currentBusinessUnit = _currentBusinessUnit.Current();
			var compare = StringComparer.Create(_loggedOnUser.CurrentUser().PermissionInformation.UICulture(), false);
			var sites = _siteRepository.LoadAll()
				.Where(site => site.BusinessUnit.Id == currentBusinessUnit.Id)
				.OrderBy(site => site.Description.Name, compare);
			var siteViewModels = new List<SiteViewModelWithTeams>();

			var currentUser = _loggedOnUser.CurrentUser();
			var myTeam = currentUser.MyTeam(date);
			var logonUserTeamId = myTeam != null && myTeam.Site.BusinessUnit == currentBusinessUnit ? myTeam.Id : null;
			var hasPermissonForLogonTeam = _permissionProvider.HasPersonPermission(permission, date, currentUser);

			foreach (var site in sites)
			{
				var siteViewModel = new SiteViewModelWithTeams
				{
					Id = site.Id.ToString(),
					Name = site.Description.Name,
					Children = new List<TeamViewModel>()
				};
				var teams = site.TeamCollection
					.OrderBy(team => team.Description.Name, compare)
					.Where(team => team.IsChoosable
								&& (_permissionProvider.HasTeamPermission(permission, date, team)
									|| (logonUserTeamId != null && logonUserTeamId == team.Id && hasPermissonForLogonTeam)));
				if (teams.Any())
				{
					siteViewModel.Children = teams.Select(team => new TeamViewModel
					{
						Id = team.Id.Value.ToString(),
						Name = team.Description.Name
					}).ToList();
					siteViewModels.Add(siteViewModel);
				}
			}

			return new BusinessUnitWithSitesViewModel
			{
				Id = currentBusinessUnit.Id ?? Guid.Empty,
				Name = currentBusinessUnit.Name,
				Children = siteViewModels,
				LogonUserTeamId = hasPermissonForLogonTeam ? logonUserTeamId : null
			};
		}

		public BusinessUnitWithSitesViewModel GetOrganizationWithPeriod(DateOnlyPeriod dateOnlyPeriod, string functionPath)
		{
			var currentBusinessUnit = _currentBusinessUnit.Current();
			var compare = StringComparer.Create(_loggedOnUser.CurrentUser().PermissionInformation.UICulture(), false);

			var items = _personSelectorReadOnlyRepository.GetOrganizationForWeb(dateOnlyPeriod);
			var currentUser = _loggedOnUser.CurrentUser();
			var myTeam = currentUser.MyTeam(dateOnlyPeriod.StartDate);
			var logonUserTeamId = myTeam != null && myTeam.Site.BusinessUnit == currentBusinessUnit ? myTeam.Id : null;
			var hasPermissonForLogonTeam = _permissionProvider.HasPersonPermission(functionPath, dateOnlyPeriod.StartDate, currentUser);
			Func<Guid, Team> getTeam = (teamId) =>
			{
				var team = new Team();
				team.SetId(teamId);
				return team;
			};

			var sites = items
				.GroupBy(item => item.SiteId)
				.Select(site => new SiteViewModelWithTeams
				{
					Id = site.Key?.ToString(),
					Name = site.First().Site,
					Children = site
						.DistinctBy(s => s.TeamId)
						.Where(c => c.TeamId.HasValue 
						&& (logonUserTeamId != null && logonUserTeamId == c.TeamId && hasPermissonForLogonTeam)
						|| _permissionProvider.HasTeamPermission(functionPath, dateOnlyPeriod.StartDate, getTeam(c.TeamId.Value)))
						.Select(t => new TeamViewModel
						{
							Id = t.TeamId.ToString(),
							Name = t.Team
						})
						.OrderBy(t => t.Name, compare)
						.ToList()
				})
				.Where(site => site.Children != null && site.Children.Any())
				.OrderBy(s => s.Name, compare)
				.ToList();

			return new BusinessUnitWithSitesViewModel
			{
				Id = currentBusinessUnit.Id ?? Guid.Empty,
				Name = currentBusinessUnit.Name,
				Children = sites,
				LogonUserTeamId = hasPermissonForLogonTeam ? logonUserTeamId : null
			};
		}
	}
}
