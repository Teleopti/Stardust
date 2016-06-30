using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ReducePrimarySkillResourcesPercentageDistribution
	{
		public void Execute(ISkillStaffPeriodHolder skillStaffPeriodHolder, IEnumerable<ISkill> primarySkills, DateTimePeriod interval, double resourcesMoved)
		{
			var totalPrimarySkillOverstaff = 0d;
			var overstaffedPrimarySkills = new List<ISkill>();
			foreach (var primarySkill in primarySkills)
			{
				var absDiff = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval).AbsoluteDifference;
				if (absDiff.IsOverstaffed())
				{
					totalPrimarySkillOverstaff += absDiff;
					overstaffedPrimarySkills.Add(primarySkill);
				}
			}

			foreach (var primarySkill in overstaffedPrimarySkills)
			{
				var skillStaffPeriod = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval);
				var overstaffForSkill = skillStaffPeriod.AbsoluteDifference;
				var percentageOverstaff = overstaffForSkill / totalPrimarySkillOverstaff;
				var resourceToSubtract = resourcesMoved * percentageOverstaff;
				skillStaffPeriod.AddResources(-resourceToSubtract);
			}
		}
	}
}