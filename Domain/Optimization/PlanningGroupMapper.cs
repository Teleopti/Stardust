using System;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupMapper
	{
		private readonly FilterMapper _filterMapper;
		public PlanningGroupMapper(FilterMapper filterMapper)
		{
			_filterMapper = filterMapper;
		}

		public PlanningGroupModel ToModel(PlanningGroup planningGroup, int agentCount)
		{
			var filterModels = planningGroup.Filters.Select(filter => _filterMapper.ToModel(filter)).ToList();

			return new PlanningGroupModel
			{
				Id = planningGroup.Id ?? Guid.Empty,
				Name = planningGroup.Name,
				Filters = filterModels,
				AgentCount = agentCount,
				PreferenceValue = planningGroup.Settings.PreferenceValue
			};
		}
	}
}