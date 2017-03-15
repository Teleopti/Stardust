using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffRulesModelPersister : IDayOffRulesModelPersister
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;
		private readonly FilterMapper _filterMapper;
		private readonly IAgentGroupRepository _agentGroupRepository;

		public DayOffRulesModelPersister(IDayOffRulesRepository dayOffRulesRepository, FilterMapper filterMapper, IAgentGroupRepository agentGroupRepository)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
			_filterMapper = filterMapper;
			_agentGroupRepository = agentGroupRepository;
		}

		public void Persist(DayOffRulesModel model)
		{
			IAgentGroup agentGroup = null;
			if (model.AgentGroupId.HasValue)
				agentGroup = _agentGroupRepository.Get(model.AgentGroupId.Value);

			if (model.Id == Guid.Empty)
			{
				var dayOffRules = model.Default ?
					DayOffRules.CreateDefault(agentGroup) :
					new DayOffRules(agentGroup);
				setProperies(dayOffRules, model);
				_dayOffRulesRepository.Add(dayOffRules);
			}
			else
			{
				var dayOffRules = _dayOffRulesRepository.Get(model.Id);
				setProperies(dayOffRules, model);
			}
		}

		public void Delete(Guid id)
		{
			var dayOffRule = _dayOffRulesRepository.Get(id);
			if (dayOffRule != null)
				_dayOffRulesRepository.Remove(dayOffRule);
		}

		private void setProperies(DayOffRules dayOffRules, DayOffRulesModel dayOffRulesModel)
		{
			dayOffRules.DayOffsPerWeek = new MinMax<int>(dayOffRulesModel.MinDayOffsPerWeek, dayOffRulesModel.MaxDayOffsPerWeek);
			dayOffRules.ConsecutiveDayOffs = new MinMax<int>(dayOffRulesModel.MinConsecutiveDayOffs, dayOffRulesModel.MaxConsecutiveDayOffs);
			dayOffRules.ConsecutiveWorkdays = new MinMax<int>(dayOffRulesModel.MinConsecutiveWorkdays, dayOffRulesModel.MaxConsecutiveWorkdays);
			dayOffRules.Name = dayOffRulesModel.Name;

			dayOffRules.ClearFilters();
			foreach (var filter in dayOffRulesModel.Filters.Select(filterModel => _filterMapper.ToEntity(filterModel)))
			{
				dayOffRules.AddFilter(filter);
			}
		}
	}
}