using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Ajax.Utilities;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
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
		private readonly ITeamRepository _teamRepository;

		public TeamsProvider(ISiteRepository siteRepository,
			ICurrentBusinessUnit currentBusinessUnit,
			IPermissionProvider permissionProvider,
			ILoggedOnUser loggedOnUser,
			IPersonSelectorReadOnlyRepository personSelectorReadOnlyRepository,
			ITeamRepository teamRepository)
		{
			_siteRepository = siteRepository;
			_currentBusinessUnit = currentBusinessUnit;
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
			_personSelectorReadOnlyRepository = personSelectorReadOnlyRepository;
			_teamRepository = teamRepository;
		}

		private IEnumerable<TeamViewModel> getTeamsForSite(ISite site)
		{
			return site.SortedTeamCollection
				.Where(team => team.IsChoosable)
				.Select(team => new TeamViewModel
				{
					Id = team.Id.Value,
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
					Id = site.Id.Value,
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

		public BusinessUnitWithSitesViewModel GetPermittedTeamHierachy(DateOnly date, string functionPath)
		{
			var compare = StringComparer.Create(_loggedOnUser.CurrentUser().PermissionInformation.UICulture(), false);
			var sites = _siteRepository.LoadAll()
				.Where(site => site.BusinessUnit.Id.GetValueOrDefault() == _currentBusinessUnit.Current().Id.GetValueOrDefault())
				.OrderBy(site => site.Description.Name, compare);
			var siteViewModels = new List<SiteViewModelWithTeams>();

			Guid? logonUserTeamId = getPermittedLogonTeam(date, functionPath);
			var validTeamIds = hasPermission(date, functionPath, logonUserTeamId,
				sites.SelectMany(s => s.TeamCollection)
					.Select(item => item.Id.Value)
					.Distinct().ToArray());

			foreach (var site in sites)
			{
				var siteViewModel = new SiteViewModelWithTeams
				{
					Id = site.Id.Value,
					Name = site.Description.Name,
					Children = new List<TeamViewModel>()
				};
				var teams = site.TeamCollection
					.OrderBy(team => team.Description.Name, compare)
					.Where(team => team.IsChoosable
								&& validTeamIds.Contains(team.Id.Value));

				if (teams.Any())
				{
					siteViewModel.Children = teams.Select(team => new TeamViewModel
					{
						Id = team.Id.Value,
						Name = team.Description.Name
					}).ToList();
					siteViewModels.Add(siteViewModel);
				}
			}

			return new BusinessUnitWithSitesViewModel
			{
				Id = _currentBusinessUnit.Current().Id ?? Guid.Empty,
				Name = _currentBusinessUnit.Current().Name,
				Children = siteViewModels,
				LogonUserTeamId = logonUserTeamId
			};
		}

		public BusinessUnitWithSitesViewModel GetOrganizationWithPeriod(DateOnlyPeriod dateOnlyPeriod, string functionPath)
		{
			var compare = StringComparer.Create(_loggedOnUser.CurrentUser().PermissionInformation.UICulture(), false);
			var items = _personSelectorReadOnlyRepository.GetOrganizationForWeb(dateOnlyPeriod);
			Guid? logonUserTeamId = getPermittedLogonTeam(dateOnlyPeriod.StartDate, functionPath);
			var validTeamIds = hasPermission(dateOnlyPeriod.StartDate, functionPath, logonUserTeamId,
					items
					.Select(item => item.TeamId.Value)
					.Distinct().ToArray());

			var sites = items
				.GroupBy(item => item.SiteId)
				.Select(site => new SiteViewModelWithTeams
				{
					Id = site.Key.Value,
					Name = site.First().Site,
					Children = site
						.DistinctBy(s => s.TeamId)
						.Where(c => validTeamIds.Contains(c.TeamId.Value))
						.Select(t => new TeamViewModel
						{
							Id = t.TeamId.Value,
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
				Id = _currentBusinessUnit.Current().Id ?? Guid.Empty,
				Name = _currentBusinessUnit.Current().Name,
				Children = sites,
				LogonUserTeamId = logonUserTeamId
			};
		}

		private IEnumerable<Guid> hasPermission(DateOnly date, string functionPath, Guid? logonUserTeamId, params Guid[] teamIds)
		{
			var teams = _teamRepository.FindTeams(teamIds);
			foreach (var team in teams)
			{
				if (_permissionProvider.HasTeamPermission(functionPath, date, team)
					|| (logonUserTeamId.HasValue && team.Id == logonUserTeamId.Value))
				{
					yield return team.Id.Value;
				}
			}

		}

		private Guid? getPermittedLogonTeam(DateOnly date, string functionPath)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var myTeam = currentUser.MyTeam(date);
			var hasPermissonForLogonTeam = _permissionProvider.HasPersonPermission(functionPath, date, currentUser);
			if (myTeam?.Id != null && myTeam.Site.BusinessUnit.Id == _currentBusinessUnit.Current()?.Id && hasPermissonForLogonTeam)
			{
				return myTeam.Id.Value;
			}
			return null;
		}
	}
}
