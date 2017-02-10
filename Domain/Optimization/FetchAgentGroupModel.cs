using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FetchAgentGroupModel : IFetchAgentGroupModel
	{
		private readonly IAgentGroupRepository _agentGroupRepository;
		private readonly AgentGroupMapper _agentGroupMapper;

		public FetchAgentGroupModel(IAgentGroupRepository agentGroupRepository, AgentGroupMapper agentGroupMapper)
		{
			_agentGroupRepository = agentGroupRepository;
			_agentGroupMapper = agentGroupMapper;
		}

		public IEnumerable<AgentGroupModel> FetchAll()
		{
			var all = _agentGroupRepository.LoadAll();

			var result = all.Select(agentGroup => _agentGroupMapper.ToModel(agentGroup)).ToList();

			return result;
		}

		public AgentGroupModel Fetch(Guid id)
		{
			var agentGroup = _agentGroupRepository.Get(id);
			if (agentGroup == null)
				throw new ArgumentException($"Cannot find AgentGroup with Id {id}");

			return _agentGroupMapper.ToModel(agentGroup);
		}
	}
}