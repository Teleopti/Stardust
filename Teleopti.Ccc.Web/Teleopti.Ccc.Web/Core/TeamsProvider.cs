using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Global.Models;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;


namespace Teleopti.Ccc.Web.Core
{
	public class TeamsProvider : ITeamsProvider
	{
		private readonly ISiteRepository _siteRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public TeamsProvider(ISiteRepository siteRepository,
			ICurrentBusinessUnit currentBusinessUnit,
			IPermissionProvider permissionProvider,
			ILoggedOnUser loggedOnUser, IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_siteRepository = siteRepository;
			_currentBusinessUnit = currentBusinessUnit;
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
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
				.Where(site => site.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault() == _currentBusinessUnit.Current().Id.GetValueOrDefault())
				.OrderBy(site => site.Description.Name, compare);
			var siteViewModels = new List<SiteViewModelWithTeams>();

			Guid? logonUserTeamId = getPermittedLogonTeam(date, functionPath);

			foreach (var site in sites)
			{
				var siteViewModel = new SiteViewModelWithTeams
				{
					Id = site.Id.GetValueOrDefault(),
					Name = site.Description.Name,
					Children = new List<TeamViewModel>()
				};
				var permittedTeams = getPermittedTeams(date, functionPath, logonUserTeamId, site.TeamCollection.ToList()).ToList();

				if (permittedTeams.Any())
				{
					siteViewModel.Children = permittedTeams
						.Where(t => t.IsChoosable)
						.OrderBy(t => t.Description.Name, compare)
						.Select(team => new TeamViewModel
						{
							Id = team.Id.GetValueOrDefault(),
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
			var groupsInMain = _groupingReadOnlyRepository.AvailableGroups(dateOnlyPeriod, Group.PageMainId);
			Guid? logonUserTeamId = getPermittedLogonTeam(dateOnlyPeriod.StartDate, functionPath);
			var sitesLookup = groupsInMain.ToLookup(g => g.SiteId);
			var sites = new List<SiteViewModelWithTeams>();
			foreach (var siteLookup in sitesLookup)
			{
				var teams = sitesLookup[siteLookup.Key];
				var permittedTeams =
					teams.Where(
							t =>
								_permissionProvider.HasOrganisationDetailPermission(functionPath, dateOnlyPeriod.StartDate, t) ||
								logonUserTeamId != null && t.TeamId == logonUserTeamId)
						.Select(t => new TeamViewModel
						{
							Id = t.TeamId.GetValueOrDefault(),
							Name = t.GroupName.Split('/').Second()
						}).OrderBy(t => t.Name, compare)
						.ToList();
				if(!permittedTeams.Any()) continue;
				sites.Add(new SiteViewModelWithTeams
				{
					Id = teams.First().SiteId.Value,
					Name = teams.First().GroupName.Split('/').First(),
					Children = permittedTeams
				});
			}

			return new BusinessUnitWithSitesViewModel
			{
				Id = _currentBusinessUnit.Current().Id ?? Guid.Empty,
				Name = _currentBusinessUnit.Current().Name,
				Children = sites.OrderBy(s => s.Name, compare).ToList(),
				LogonUserTeamId = logonUserTeamId
			};
		}
		private IEnumerable<ITeam> getPermittedTeams(DateOnly date, string functionPath, Guid? logonUserTeamId, IList<ITeam> teams)
		{
			foreach (var team in teams)
			{
				if (_permissionProvider.HasTeamPermission(functionPath, date, team)
					|| logonUserTeamId.HasValue && team.Id == logonUserTeamId.Value)
				{
					yield return team;
				}
			}

		}

		private Guid? getPermittedLogonTeam(DateOnly date, string functionPath)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var myTeam = currentUser.MyTeam(date);
			if (myTeam?.Id != null
				&& myTeam.Site.GetOrFillWithBusinessUnit_DONTUSE().Id == _currentBusinessUnit.Current()?.Id
				&& _permissionProvider.HasPersonPermission(functionPath, date, currentUser))
			{
				return myTeam.Id.Value;
			}
			return null;
		}
	}
}
