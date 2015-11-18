using System;
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
			foreach (var modelfilter in dayOffRulesModel.Filters)
			{
				switch (modelfilter.FilterType)
				{
					case FilterModel.ContractFilterType:
						var contract = _contractRepository.Get(modelfilter.Id);
						var contractFilter = new ContractFilter(contract);
						dayOffRules.AddFilter(contractFilter);
						break;
					case FilterModel.SiteFilterType:
						var site = _siteRepository.Get(modelfilter.Id);
						var siteFilter = new SiteFilter(site);
						dayOffRules.AddFilter(siteFilter);
						break;
					case FilterModel.TeamFilterType:
						var team = _teamRepository.Get(modelfilter.Id);
						var teamFilter = new TeamFilter(team);
						dayOffRules.AddFilter(teamFilter);
						break;
					default:
						throw new NotSupportedException(string.Format("Unknown filter type {0}", modelfilter.FilterType));
				}
			}
		}
	}
}