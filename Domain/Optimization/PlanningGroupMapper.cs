using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupMapper
	{
		private readonly FilterMapper _filterMapper;
		public PlanningGroupMapper(FilterMapper filterMapper)
		{
			_filterMapper = filterMapper;
		}

		public PlanningGroupModel ToModel(IPlanningGroup planningGroup, int agentCount)
		{
			var filterModels = planningGroup.Filters.Select(filter => _filterMapper.ToModel(filter)).ToList();

			return new PlanningGroupModel
			{
				Id = planningGroup.Id ?? Guid.Empty,
				Name = planningGroup.Name,
				Filters = filterModels,
				AgentCount = agentCount
			};
		}
	}
}