using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ReducePrimarySkillResourcesPercentageDistribution
	{
		public void Execute(ISkillStaffPeriodHolder skillStaffPeriodHolder, IEnumerable<ISkill> primarySkills, DateTimePeriod interval, double resourcesMoved)
		{
			var primarySkillOverstaff = primarySkills.Sum(primarySkill => skillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval, 0).AbsoluteDifference);
			if (primarySkillOverstaff.IsZero())
				return;

			foreach (var primarySkill in primarySkills)
			{
				var skillStaffPeriod = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval, 0);
				var overstaffForSkill = skillStaffPeriod.AbsoluteDifference;
				var percentageOverstaff = overstaffForSkill / primarySkillOverstaff;
				var resourceToSubtract = resourcesMoved * percentageOverstaff;
				skillStaffPeriod.AddResources(-resourceToSubtract);
			}
		}
	}
}