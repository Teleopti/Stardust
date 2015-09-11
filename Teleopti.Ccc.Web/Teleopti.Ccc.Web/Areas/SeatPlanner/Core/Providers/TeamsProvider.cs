using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class TeamsProvider : ITeamsProvider
	{
		private readonly ISiteRepository _siteRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public TeamsProvider(ISiteRepository siteRepository, IBusinessUnitRepository businessUnitRepository, ICurrentBusinessUnit currentBusinessUnit)
		{
			_siteRepository = siteRepository;
			_businessUnitRepository = businessUnitRepository;
			_currentBusinessUnit = currentBusinessUnit;
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
			var currentBusinessUnit = _businessUnitRepository.Get(_currentBusinessUnit.Current().Id.GetValueOrDefault());
			var sites = _siteRepository.LoadAll().OrderBy (site => site.Description.Name);
			var siteViewModels = new List<SiteViewModelWithTeams>();

			foreach (var site in sites)
			{
				var siteViewModel = new SiteViewModelWithTeams()
				{
					Id = site.Id.ToString(),
					Name = site.Description.Name
				};

				siteViewModels.Add(siteViewModel);
				var teamViewModels = getTeamsForSite(site);
				siteViewModel.Children.AddRange(teamViewModels);
			}

			return new BusinessUnitWithSitesViewModel()
			{
				Id = currentBusinessUnit.Id ?? Guid.Empty,
				Name = currentBusinessUnit.Name,
				Children = siteViewModels
			};

		}
	}
}
