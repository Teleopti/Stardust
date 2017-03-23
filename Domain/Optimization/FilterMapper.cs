using System;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FilterMapper
	{
		private readonly IContractRepository _contractRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly ISkillRepository _skillRepository;

		public FilterMapper(IContractRepository contractRepository, 
										ITeamRepository teamRepository, 
										ISiteRepository siteRepository, 
										ISkillRepository skillRepository)
		{
			_contractRepository = contractRepository;
			_teamRepository = teamRepository;
			_siteRepository = siteRepository;
			_skillRepository = skillRepository;
		}


		public FilterModel ToModel(IFilter filter)
		{
			var contractFilter = filter as ContractFilter;
			if (contractFilter != null)
			{
				return new FilterModel
				{
					FilterType = FilterModel.ContractFilterType,
					Id = contractFilter.Contract.Id.Value,
					Name = contractFilter.Contract.Description.Name
				};
			}

			var teamFilter = filter as TeamFilter;
			if (teamFilter != null)
			{
				return new FilterModel
				{
					FilterType = FilterModel.TeamFilterType,
					Id = teamFilter.Team.Id.Value,
					Name = teamFilter.Team.SiteAndTeam
				};
			}

			var siteFilter = filter as SiteFilter;
			if (siteFilter != null)
			{
				return new FilterModel
				{
					FilterType = FilterModel.SiteFilterType,
					Id = siteFilter.Site.Id.Value,
					Name = siteFilter.Site.Description.Name
				};
			}

			var skillFilter = filter as SkillFilter;
			if (skillFilter != null)
			{
				return new FilterModel
				{
					FilterType = FilterModel.SkillFilterType,
					Id = skillFilter.Skill.Id.Value,
					Name = skillFilter.Skill.Name
				};
			}

			throw new NotSupportedException("Unknown filter type" + filter);
		}

		public IFilter ToEntity(FilterModel filterModel)
		{
			switch (filterModel.FilterType)
			{
				case FilterModel.ContractFilterType:
					return new ContractFilter(_contractRepository.Get(filterModel.Id));
				case FilterModel.SiteFilterType:
					return new SiteFilter(_siteRepository.Get(filterModel.Id));
				case FilterModel.TeamFilterType:
					return new TeamFilter(_teamRepository.Get(filterModel.Id));
				case FilterModel.SkillFilterType:
					return new SkillFilter(_skillRepository.Get(filterModel.Id));
				default:
					throw new NotSupportedException($"Unknown filter type {filterModel.FilterType}");
			}
		}
	}
}