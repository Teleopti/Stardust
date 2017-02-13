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
		private readonly ISkillRepository _skillRepository;

		public FindFilter(IContractRepository contractRepository, ITeamRepository teamRepository, ISiteRepository siteRepository, ISkillRepository skillRepository)
		{
			_contractRepository = contractRepository;
			_teamRepository = teamRepository;
			_siteRepository = siteRepository;
			_skillRepository = skillRepository;
		}

		public IEnumerable<FindFilterResult> Search(string searchString, int maxHits)
		{
			if (searchString.IsEmpty())
				return Enumerable.Empty<FindFilterResult>();

			var itemsLeftToLoad = maxHits;
			var contractHits = searchContract(searchString, itemsLeftToLoad);
			itemsLeftToLoad = itemsLeftToLoad - contractHits.Count();
			var teamHits = searchTeam(searchString, itemsLeftToLoad);
			itemsLeftToLoad = itemsLeftToLoad - teamHits.Count();
			var siteHits = searchSite(searchString, itemsLeftToLoad);

			return contractHits.Union(teamHits).Union(siteHits);
		}

		private IEnumerable<FindFilterResult> searchContract(string searchString, int itemsLeftToLoad)
		{
			return _contractRepository.FindContractsContain(searchString, itemsLeftToLoad)
				.Select(contract => new FindFilterResult {FilterType = FilterModel.ContractFilterType, Id = contract.Id.Value, Name = contract.Description.Name});
		}

		private IEnumerable<FindFilterResult> searchTeam(string searchString, int itemsLeftToLoad)
		{
			return _teamRepository.FindTeamsContain(searchString, itemsLeftToLoad)
				.Select(team => new FindFilterResult {FilterType = FilterModel.TeamFilterType, Id = team.Id.Value, Name = team.Description.Name});
		}

		private IEnumerable<FindFilterResult> searchSite(string searchString, int itemsLeftToLoad)
		{
			return _siteRepository.FindSitesContain(searchString, itemsLeftToLoad)
				.Select(site => new FindFilterResult { FilterType = FilterModel.SiteFilterType, Id = site.Id.Value, Name = site.Description.Name });
		}

		private IEnumerable<FindFilterResult> searchSkill(string searchString, int itemsLeftToLoad)
		{
			return _skillRepository.FindSkillsContain(searchString, itemsLeftToLoad)
				.Select(skill => new FindFilterResult { FilterType = FilterModel.SkillFilterType, Id = skill.Id.Value, Name = skill.Name });
		}

		public IEnumerable<FindFilterResult> SearchForAgentGroup(string searchString, int maxHits)
		{
			if (searchString.IsEmpty())
				return Enumerable.Empty<FindFilterResult>();

			var itemsLeftToLoad = maxHits;
			var siteHits = searchSite(searchString, itemsLeftToLoad);
			itemsLeftToLoad = itemsLeftToLoad - siteHits.Count();
			var teamHits = searchTeam(searchString, itemsLeftToLoad);
			itemsLeftToLoad = itemsLeftToLoad - teamHits.Count();
			var skillHits = searchSkill(searchString, itemsLeftToLoad);
			itemsLeftToLoad = itemsLeftToLoad - skillHits.Count();
			var contractHits = searchContract(searchString, itemsLeftToLoad);
			return siteHits.Union(teamHits).Union(skillHits).Union(contractHits);
		}
	}
}