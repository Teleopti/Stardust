using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class PrimarySkillOverstaff
	{
		public ShovelResourcesState AvailableSum(ISkillStaffPeriodHolder skillStaffPeriodHolder, IEnumerable<CascadingSkillGroup> allSkillGroups, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval)
		{
			var primarySkillsExistsButTheyAreAllClosed = true;
			var dic = new Dictionary<ISkill, double>();

			foreach (var primarySkill in skillGroupsWithSameIndex.First().PrimarySkills)
			{
				ISkillStaffPeriod primarySkillStaffPeriod;
				if (!skillStaffPeriodHolder.TryGetSkillStaffPeriod(primarySkill, interval, out primarySkillStaffPeriod))
					continue;
				primarySkillsExistsButTheyAreAllClosed = false;
				var primarySkillOverstaff = primarySkillStaffPeriod.AbsoluteDifference;
				if (!primarySkillOverstaff.IsOverstaffed())
					continue;

				var resourcesOnOtherSkillGroupsContainingThisPrimarySkill = 0d;
				foreach (var otherSkillGroup in allSkillGroups.Where(x => !skillGroupsWithSameIndex.Contains(x) && x.PrimarySkills.Contains(primarySkill)))
				{
					var resourcesOnOtherSkillGroup = otherSkillGroup.RemainingResources;
					foreach (var otherPrimarySkill in otherSkillGroup.PrimarySkills.Where(x => !x.Equals(primarySkill)))
					{
						resourcesOnOtherSkillGroup -= skillStaffPeriodHolder.SkillStaffPeriodOrDefault(otherPrimarySkill, interval).CalculatedResource;
					}
					resourcesOnOtherSkillGroupsContainingThisPrimarySkill += resourcesOnOtherSkillGroup;
				}

				var otherSkillGroupOverstaff = Math.Max(resourcesOnOtherSkillGroupsContainingThisPrimarySkill - primarySkillStaffPeriod.FStaff, 0);
				dic.Add(primarySkill, primarySkillOverstaff - otherSkillGroupOverstaff);
			}

			if (primarySkillsExistsButTheyAreAllClosed)
			{
				//TODO: suspicious code here - bug?
				foreach (var skillGroup in skillGroupsWithSameIndex)
				{
					dic.Add(skillGroup.PrimarySkills.First(), skillGroup.RemainingResources);
					break;
				}
			}

			return new ShovelResourcesState(dic, new ResourceDistributionForSkillGroupsWithSameIndex(skillStaffPeriodHolder, skillGroupsWithSameIndex, interval)); 
		}
	}
}