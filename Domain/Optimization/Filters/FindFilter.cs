﻿using System.Collections.Generic;
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

		public IEnumerable<FindFilterResult> Search(string searchString)
		{
			if (searchString.IsEmpty())
				return Enumerable.Empty<FindFilterResult>();

			var contractHits = _contractRepository.FindContractsStartWith(searchString)
				.Select(contract => new FindFilterResult {FilterType = "contract", Id = contract.Id.Value, Name = contract.Description.Name});
			var teamHits = _teamRepository.FindTeamsStartWith(searchString)
				.Select(team => new FindFilterResult {FilterType = "team", Id = team.Id.Value, Name = team.Description.Name});
			var siteHits = _siteRepository.FindSitesStartWith(searchString)
				.Select(site => new FindFilterResult { FilterType = "site", Id = site.Id.Value, Name = site.Description.Name });

			return contractHits.Union(teamHits).Union(siteHits);
		}
	}
}