using System;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupMapper
	{
		private readonly FilterMapper _filterMapper;
		private readonly PlanningGroupSettingsMapper _planningGroupSettingsMapper;

		public PlanningGroupMapper(FilterMapper filterMapper, PlanningGroupSettingsMapper planningGroupSettingsMapper)
		{
			_filterMapper = filterMapper;
			_planningGroupSettingsMapper = planningGroupSettingsMapper;
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
				PreferenceValue = planningGroup.Settings.PreferenceValue.Value,
				Settings = planningGroup.Settings.Select(planningGroupSettings => _planningGroupSettingsMapper.ToModel(planningGroupSettings)).ToList()
			};
		}
	}
}