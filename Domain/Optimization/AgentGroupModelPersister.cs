using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AgentGroupModelPersister : IAgentGroupModelPersister
	{
		private readonly IAgentGroupRepository _agentGroupRepository;
		private readonly IDayOffRulesRepository _dayOffRulesRepository;
		private readonly FilterMapper _filterMapper;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public AgentGroupModelPersister(IAgentGroupRepository agentGroupRepository, FilterMapper filterMapper, IDayOffRulesRepository dayOffRulesRepository, IPlanningPeriodRepository planningPeriodRepository)
		{
			_agentGroupRepository = agentGroupRepository;
			_filterMapper = filterMapper;
			_dayOffRulesRepository = dayOffRulesRepository;
			_planningPeriodRepository = planningPeriodRepository;
		}

		public void Persist(AgentGroupModel agentGroupModel)
		{
			if (agentGroupModel.Id == Guid.Empty)
			{
				var agentGroup = new AgentGroup();
				setProperties(agentGroup, agentGroupModel);
				_agentGroupRepository.Add(agentGroup);
				_dayOffRulesRepository.Add(DayOffRules.CreateDefault(agentGroup));
			}
			else
			{
				var agentGroup = _agentGroupRepository.Get(agentGroupModel.Id);
				setProperties(agentGroup, agentGroupModel);
			}
		}

		private void setProperties(IAgentGroup agentGroup, AgentGroupModel agentGroupModel)
		{
			agentGroup.ChangeName(agentGroupModel.Name);

			agentGroup.ClearFilters();
			foreach (var filter in agentGroupModel.Filters.Select(filterModel => _filterMapper.ToEntity(filterModel)))
			{
				agentGroup.AddFilter(filter);
			}
		}

		public void Delete(Guid agentGroupId)
		{
			var agentGroup = _agentGroupRepository.Get(agentGroupId);
			if (agentGroup == null) return;
			_dayOffRulesRepository.RemoveForAgentGroup(agentGroup);
			_agentGroupRepository.Remove(agentGroup);
		}
	}
}