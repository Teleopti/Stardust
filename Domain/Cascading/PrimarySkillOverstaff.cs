using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class PrimarySkillOverstaff
	{
		public ShovelResourcesState AvailableSum(ISkillStaffPeriodHolder skillStaffPeriodHolder, IEnumerable<CascadingSkillGroup> allSkillGroups, CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			var primarySkillsExistsButTheyAreAllClosed = true;
			var dic = new Dictionary<ISkill, double>();
			foreach (var primarySkill in skillGroup.PrimarySkills)
			{
				ISkillStaffPeriod primarySkillStaffPeriod;
				if (!skillStaffPeriodHolder.TryGetSkillStaffPeriod(primarySkill, interval, out primarySkillStaffPeriod))
						continue; 
				primarySkillsExistsButTheyAreAllClosed = false;
				var primarySkillOverstaff = primarySkillStaffPeriod.AbsoluteDifference;
				if (!primarySkillOverstaff.IsOverstaffed())
					continue;

				var resourcesOnOtherSkillGroupsContainingThisPrimarySkill = allSkillGroups
					.Where(x => !x.Equals(skillGroup) && x.PrimarySkills.Contains(primarySkill))
					.Sum(otherSkillGroup => otherSkillGroup.RemainingResources);

				var otherSkillGroupOverstaff = Math.Max(resourcesOnOtherSkillGroupsContainingThisPrimarySkill - primarySkillStaffPeriod.FStaff, 0);
				dic.Add(primarySkill, primarySkillOverstaff - otherSkillGroupOverstaff);
			}

			if (primarySkillsExistsButTheyAreAllClosed)
			{
				dic.Add(skillGroup.PrimarySkills.First(), skillGroup.RemainingResources); //shouldn't really matter what primary skill we choose here (if multiple).
			}

			return new ShovelResourcesState(dic, skillGroup);
		}
	}
}