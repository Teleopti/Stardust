using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ReducePrimarySkillResourcesPercentageDistribution
	{
		public void Execute(ISkillStaffPeriodHolder skillStaffPeriodHolder, IEnumerable<ISkill> primarySkills, DateTimePeriod interval, ShovelResourcesState state)
		{
			var totalPrimarySkillOverstaff = state.TotalResourcesAtStart;

			foreach (var keyValueForPrimarySkillAndResources in state.ResourcesAvailableForPrimarySkill())
			{
				var primarySkill = keyValueForPrimarySkillAndResources.Key;
				var skillStaffPeriod = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval);
				var overstaffForSkill = keyValueForPrimarySkillAndResources.Value;
				var percentageOverstaff = overstaffForSkill / totalPrimarySkillOverstaff;
				var resourceToSubtract = state.ResourcesMoved * percentageOverstaff;
				skillStaffPeriod.AddResources(-resourceToSubtract);
			}
		}
	}
}