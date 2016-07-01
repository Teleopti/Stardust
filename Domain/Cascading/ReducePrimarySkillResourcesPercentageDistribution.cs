using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ReducePrimarySkillResourcesPercentageDistribution
	{
		public void Execute(ISkillStaffPeriodHolder skillStaffPeriodHolder, IEnumerable<ISkill> primarySkills, DateTimePeriod interval, ShovelResourcesState state)
		{
			if (jumpOutEarly(state))
				return;

			foreach (var keyValueForPrimarySkillAndResources in state.ResourcesAvailableForPrimarySkill())
			{
				var primarySkill = keyValueForPrimarySkillAndResources.Key;
				var skillStaffPeriod = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval);
				var overstaffForSkill = keyValueForPrimarySkillAndResources.Value;
				var percentageOverstaff = overstaffForSkill / state.TotalResourcesAtStart;
				var resourceToSubtract = state.ResourcesMoved * percentageOverstaff;
				skillStaffPeriod.AddResources(-resourceToSubtract);
			}
		}

		private bool jumpOutEarly(ShovelResourcesState state)
		{
			const double tolerance = 0.0000000001;
			return Math.Abs(state.ResourcesMoved) < tolerance || Math.Abs(state.TotalResourcesAtStart) < tolerance;
		}
	}
}