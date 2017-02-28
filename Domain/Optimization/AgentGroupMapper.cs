using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AgentGroupMapper
	{
		private readonly FilterMapper _filterMapper;
		public AgentGroupMapper(FilterMapper filterMapper)
		{
			_filterMapper = filterMapper;
		}

		public AgentGroupModel ToModel(IAgentGroup agentGroup)
		{
			var filterModels = agentGroup.Filters.Select(filter => _filterMapper.ToModel(filter)).ToList();

			return new AgentGroupModel
			{
				Id = agentGroup.Id ?? Guid.Empty,
				Name = agentGroup.Name,
				Filters = filterModels
			};
		}
	}
}