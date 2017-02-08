using System;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AgentGroupModelPersister : IAgentGroupModelPersister
	{
		private readonly IAgentGroupRepository _agentGroupRepository;
		private readonly FilterMapper _filterMapper;

		public AgentGroupModelPersister(IAgentGroupRepository agentGroupRepository, FilterMapper filterMapper)
		{
			_agentGroupRepository = agentGroupRepository;
			_filterMapper = filterMapper;
		}

		public void Persist(AgentGroupModel agentGroupModel)
		{
			if (agentGroupModel.Id == Guid.Empty)
			{
				var agentGroup = new AgentGroup();
				setProperties(agentGroup, agentGroupModel);
				_agentGroupRepository.Add(agentGroup);
			}
			else
			{
				var agentGroup = _agentGroupRepository.Get(agentGroupModel.Id);
				setProperties(agentGroup, agentGroupModel);
			}

		}

		private void setProperties(AgentGroup agentGroup, AgentGroupModel agentGroupModel)
		{
			agentGroup.Name = agentGroupModel.Name;

			agentGroup.ClearFilters();
			foreach (var filter in agentGroupModel.Filters.Select(filterModel => _filterMapper.ToEntity(filterModel)))
			{
				agentGroup.AddFilter(filter);
			}
		}
	}
}