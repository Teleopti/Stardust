using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffRulesMapper
	{
		public DayOffRulesModel ToModel(DayOffRules dayOffRules)
		{
			return new DayOffRulesModel
			{
				MinConsecutiveWorkdays = dayOffRules.ConsecutiveWorkdays.Minimum,
				MaxConsecutiveWorkdays = dayOffRules.ConsecutiveWorkdays.Maximum,
				MinDayOffsPerWeek = dayOffRules.DayOffsPerWeek.Minimum,
				MaxDayOffsPerWeek = dayOffRules.DayOffsPerWeek.Maximum,
				MinConsecutiveDayOffs = dayOffRules.ConsecutiveDayOffs.Minimum,
				MaxConsecutiveDayOffs = dayOffRules.ConsecutiveDayOffs.Maximum,
				Id = dayOffRules.Id ?? Guid.Empty,
				Default = dayOffRules.Default
			};
		}
	}
}