using System;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffRulesMapper
	{
		private readonly FilterMapper _filterMapper;

		public DayOffRulesMapper(FilterMapper filterMapper)
		{
			_filterMapper = filterMapper;
		}

		public DayOffRulesModel ToModel(PlanningGroupSettings planningGroupSettings)
		{
			var filterModels = planningGroupSettings.Filters.Select(filter => _filterMapper.ToModel(filter)).ToList();
			
			return new DayOffRulesModel
			{
				MinConsecutiveWorkdays = planningGroupSettings.ConsecutiveWorkdays.Minimum,
				MaxConsecutiveWorkdays = planningGroupSettings.ConsecutiveWorkdays.Maximum,
				MinDayOffsPerWeek = planningGroupSettings.DayOffsPerWeek.Minimum,
				MaxDayOffsPerWeek = planningGroupSettings.DayOffsPerWeek.Maximum,
				MinConsecutiveDayOffs = planningGroupSettings.ConsecutiveDayOffs.Minimum,
				MaxConsecutiveDayOffs = planningGroupSettings.ConsecutiveDayOffs.Maximum,
				Id = planningGroupSettings.Id ?? Guid.Empty,
				Default = planningGroupSettings.Default,
				Name = planningGroupSettings.Name,
				Filters = filterModels,
				PlanningGroupId = planningGroupSettings.PlanningGroup?.Id
			};
		}
	}
}