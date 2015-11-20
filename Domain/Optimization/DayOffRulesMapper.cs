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

		public DayOffRulesModel ToModel(DayOffRules dayOffRules)
		{
			var filterModels = dayOffRules.Filters.Select(filter => _filterMapper.ToModel(filter)).ToList();

			return new DayOffRulesModel
			{
				MinConsecutiveWorkdays = dayOffRules.ConsecutiveWorkdays.Minimum,
				MaxConsecutiveWorkdays = dayOffRules.ConsecutiveWorkdays.Maximum,
				MinDayOffsPerWeek = dayOffRules.DayOffsPerWeek.Minimum,
				MaxDayOffsPerWeek = dayOffRules.DayOffsPerWeek.Maximum,
				MinConsecutiveDayOffs = dayOffRules.ConsecutiveDayOffs.Minimum,
				MaxConsecutiveDayOffs = dayOffRules.ConsecutiveDayOffs.Maximum,
				Id = dayOffRules.Id ?? Guid.Empty,
				Default = dayOffRules.Default,
				Name = dayOffRules.Name,
				Filters = filterModels
			};
		}
	}
}