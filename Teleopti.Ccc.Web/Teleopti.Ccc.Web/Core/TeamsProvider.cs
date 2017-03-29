using System;
using System.Collections.Generic;
using System.Linq;
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

		public TeamsProvider(ISiteRepository siteRepository, ICurrentBusinessUnit currentBusinessUnit, IPermissionProvider permissionProvider, ILoggedOnUser loggedOnUser)
		{
			_siteRepository = siteRepository;
			_currentBusinessUnit = currentBusinessUnit;
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<TeamViewModel> Get(string siteId)
		{
			var site = _siteRepository.Get(new Guid(siteId));
			return getTeamsForSite(site);
		}

		private IEnumerable<TeamViewModel> getTeamsForSite(ISite site)
		{
			return site.SortedTeamCollection
				.Where (team => team.IsChoosable)
				.Select(team => new TeamViewModel
				{
						Id = team.Id.Value.ToString(),
						Name = team.Description.Name
				
				});
		}

		public BusinessUnitWithSitesViewModel GetTeamHierarchy()
		{
			var sites = _siteRepository.LoadAll().OrderBy (site => site.Description.Name);
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
			var sites = _siteRepository.LoadAll().Where(site => site.BusinessUnit.Id == currentBusinessUnit.Id).OrderBy(site => site.Description.Name);
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
				var teams = site.SortedTeamCollection.Where(t => t.IsChoosable);
				foreach (var team in teams)
				{
					if (_permissionProvider.HasTeamPermission(permission, date, team))
					{
						siteViewModel.Children.Add(new TeamViewModel
						{
							Id = team.Id.Value.ToString(),
							Name = team.Description.Name
						});
					}
					else if (logonUserTeamId != null && logonUserTeamId == team.Id
					       && hasPermissonForLogonTeam)
					{
						siteViewModel.Children.Add(new TeamViewModel
						{
							Id = team.Id.Value.ToString(),
							Name = team.Description.Name
						});
					}
				}
				if (siteViewModel.Children.Any())
				{
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
	}
}
