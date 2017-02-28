using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Interfaces;

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

		private void setProperties(IAgentGroup agentGroup, AgentGroupModel agentGroupModel)
		{
			agentGroup.ChangeName(agentGroupModel.Name);

			agentGroup.ClearFilters();
			foreach (var filter in agentGroupModel.Filters.Select(filterModel => _filterMapper.ToEntity(filterModel)))
			{
				agentGroup.AddFilter(filter);
			}
		}

		public void Delete(Guid id)
		{
			var agentGroup = _agentGroupRepository.Get(id);
			if (agentGroup != null)
				_agentGroupRepository.Remove(agentGroup);
		}
	}
}