using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Optimization.Filters
{
	public class FindFilter
	{
		private readonly IContractRepository _contractRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly ISiteRepository _siteRepository;

		public FindFilter(IContractRepository contractRepository, ITeamRepository teamRepository, ISiteRepository siteRepository)
		{
			_contractRepository = contractRepository;
			_teamRepository = teamRepository;
			_siteRepository = siteRepository;
		}

		public IEnumerable<FindFilterResult> Search(string searchString, int maxHits)
		{
			if (searchString.IsEmpty())
				return Enumerable.Empty<FindFilterResult>();

			var itemsLeftToLoad = maxHits;
			var contractHits = _contractRepository.FindContractsContain(searchString, itemsLeftToLoad)
				.Select(contract => new FindFilterResult {FilterType = FilterModel.ContractFilterType, Id = contract.Id.Value, Name = contract.Description.Name});
			itemsLeftToLoad = itemsLeftToLoad - contractHits.Count();
			var teamHits = _teamRepository.FindTeamsContain(searchString, itemsLeftToLoad)
				.Select(team => new FindFilterResult {FilterType = FilterModel.TeamFilterType, Id = team.Id.Value, Name = team.Description.Name});
			itemsLeftToLoad = itemsLeftToLoad - teamHits.Count();
			var siteHits = _siteRepository.FindSitesContain(searchString, itemsLeftToLoad)
				.Select(site => new FindFilterResult { FilterType = FilterModel.SiteFilterType, Id = site.Id.Value, Name = site.Description.Name });

			return contractHits.Union(teamHits).Union(siteHits);
		}
	}
}