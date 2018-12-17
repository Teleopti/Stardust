using System.Collections.Generic;
using System.Linq;
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
			var results = new List<FindFilterResult>();
			results = results.Union(searchContract(searchString, maxHits - results.Count)).ToList();
			results = results.Union(searchSite(searchString, maxHits - results.Count)).ToList();
			results = results.Union(searchTeam(searchString, maxHits - results.Count)).ToList();
			return results;
		}

		private IEnumerable<FindFilterResult> searchContract(string searchString, int itemsLeftToLoad)
		{
			return _contractRepository.FindContractsContain(searchString, itemsLeftToLoad)
				.Select(contract => new FindFilterResult(contract));
		}

		private IEnumerable<FindFilterResult> searchTeam(string searchString, int itemsLeftToLoad)
		{
			return _teamRepository.FindTeamsContain(searchString, itemsLeftToLoad)
				.Select(team => new FindFilterResult(team));
		}

		private IEnumerable<FindFilterResult> searchSite(string searchString, int itemsLeftToLoad)
		{
			var sites = _siteRepository.FindSitesContain(searchString, itemsLeftToLoad).ToList();
			var siteFilters= sites.Select(site => new FindFilterResult(site))
				.ToList();

			var left = itemsLeftToLoad - siteFilters.Count;
			var teamFilters = new List<FindFilterResult>();
			foreach (var team in sites.SelectMany(site => site.TeamCollection))
			{
				if (left < 1)
					break;
				teamFilters.Add(new FindFilterResult(team));
				left--;
			}
			return siteFilters.Union(teamFilters);
		}

		private IEnumerable<FindFilterResult> searchSkill(string searchString, int itemsLeftToLoad)
		{
			return _skillRepository.FindSkillsContain(searchString, itemsLeftToLoad)
				.Select(skill => new FindFilterResult(skill));
		}

		public IEnumerable<FindFilterResult> SearchForPlanningGroup(string searchString, int maxHits)
		{
			var results = new List<FindFilterResult>();
			results = results.Union(searchSite(searchString, maxHits - results.Count)).ToList();
			results = results.Union(searchTeam(searchString, maxHits - results.Count)).ToList();
			results = results.Union(searchContract(searchString, maxHits - results.Count)).ToList();
			results = results.Union(searchSkill(searchString, maxHits - results.Count)).ToList();
			return results;
		}
	}
}