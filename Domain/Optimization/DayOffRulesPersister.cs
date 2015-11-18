using System;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffRulesModelPersister : IDayOffRulesModelPersister
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;
		private readonly IContractRepository _contractRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamRepository _teamRepository;

		public DayOffRulesModelPersister(IDayOffRulesRepository dayOffRulesRepository,
																		IContractRepository contractRepository,
																		ISiteRepository siteRepository,
																		ITeamRepository teamRepository)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
			_contractRepository = contractRepository;
			_siteRepository = siteRepository;
			_teamRepository = teamRepository;
		}

		public void Persist(DayOffRulesModel model)
		{
			if (model.Id == Guid.Empty)
			{
				var dayOffRules = model.Default ?
					DayOffRules.CreateDefault() :
					new DayOffRules();
				setProperies(dayOffRules, model);
				_dayOffRulesRepository.Add(dayOffRules);
			}
			else
			{
				var dayOffRules = _dayOffRulesRepository.Get(model.Id);
				setProperies(dayOffRules, model);
			}
		}

		private void setProperies(DayOffRules dayOffRules, DayOffRulesModel dayOffRulesModel)
		{
			dayOffRules.DayOffsPerWeek = new MinMax<int>(dayOffRulesModel.MinDayOffsPerWeek, dayOffRulesModel.MaxDayOffsPerWeek);
			dayOffRules.ConsecutiveDayOffs = new MinMax<int>(dayOffRulesModel.MinConsecutiveDayOffs, dayOffRulesModel.MaxConsecutiveDayOffs);
			dayOffRules.ConsecutiveWorkdays = new MinMax<int>(dayOffRulesModel.MinConsecutiveWorkdays, dayOffRulesModel.MaxConsecutiveWorkdays);
			dayOffRules.Name = dayOffRulesModel.Name;

			//TODO: this is only correct when insert/new dayoffRules
			foreach (var filter in dayOffRulesModel.Filters.Select(createFilter))
			{
				dayOffRules.AddFilter(filter);
			}
		}

		private IFilter createFilter(FilterModel filterModel)
		{
			switch (filterModel.FilterType)
			{
				case FilterModel.ContractFilterType:
					return new ContractFilter(_contractRepository.Get(filterModel.Id));
				case FilterModel.SiteFilterType:
					return new SiteFilter(_siteRepository.Get(filterModel.Id));
				case FilterModel.TeamFilterType:
					return new TeamFilter(_teamRepository.Get(filterModel.Id));
				default:
					throw new NotSupportedException(string.Format("Unknown filter type {0}", filterModel.FilterType));
			}
		}
	}
}