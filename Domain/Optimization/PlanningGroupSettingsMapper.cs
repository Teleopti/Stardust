using System;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupSettingsMapper
	{
		private readonly FilterMapper _filterMapper;

		public PlanningGroupSettingsMapper(FilterMapper filterMapper)
		{
			_filterMapper = filterMapper;
		}

		public PlanningGroupSettingsModel ToModel(PlanningGroupSettings planningGroupSettings)
		{
			var filterModels = planningGroupSettings.Filters.Select(filter => _filterMapper.ToModel(filter)).ToList();
			
			return new PlanningGroupSettingsModel
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
				PlanningGroupId = planningGroupSettings.Parent.Id.Value,
				BlockFinderType = planningGroupSettings.BlockFinderType,
				BlockSameShiftCategory = planningGroupSettings.BlockSameShiftCategory,
				BlockSameShift = planningGroupSettings.BlockSameShift,
				BlockSameStartTime = planningGroupSettings.BlockSameStartTime,
				Priority = planningGroupSettings.Priority,
				MinFullWeekendsOff = planningGroupSettings.FullWeekendsOff.Minimum,
				MaxFullWeekendsOff = planningGroupSettings.FullWeekendsOff.Maximum,
				MinWeekendDaysOff = planningGroupSettings.WeekendDaysOff.Minimum,
				MaxWeekendDaysOff = planningGroupSettings.WeekendDaysOff.Maximum,
			};
		}
	}
}