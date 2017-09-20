using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class PrimarySkillOverstaff
	{
		public ShovelResourcesState AvailableSum(IShovelResourceData shovelResourceData, 
			IEnumerable<CascadingSkillSet> allSkillGroups, 
			IEnumerable<CascadingSkillSet> skillGroupsWithSameIndex, 
			DateTimePeriod interval)
		{
			var primarySkillsExistsButTheyAreAllClosed = true;
			var dic = new Dictionary<ISkill, double>();

			foreach (var primarySkill in skillGroupsWithSameIndex.First().PrimarySkills)
			{
				IShovelResourceDataForInterval shovelResourceDataForInterval;
				if (!shovelResourceData.TryGetDataForInterval(primarySkill, interval, out shovelResourceDataForInterval))
					continue;
				primarySkillsExistsButTheyAreAllClosed = false;
				var primarySkillOverstaff = shovelResourceDataForInterval.AbsoluteDifference;
				if (!primarySkillOverstaff.IsOverstaffed())
					continue;

				var resourcesOnOtherSkillGroupsContainingThisPrimarySkill = 0d;
				foreach (var otherSkillGroup in allSkillGroups.Where(x => !skillGroupsWithSameIndex.Contains(x) && x.PrimarySkills.Contains(primarySkill)))
				{
					var resourcesOnOtherSkillGroup = otherSkillGroup.RemainingResources;
					foreach (var otherPrimarySkill in otherSkillGroup.PrimarySkills.Where(x => !x.Equals(primarySkill)))
					{
						resourcesOnOtherSkillGroup -= shovelResourceData.GetDataForInterval(otherPrimarySkill, interval).CalculatedResource;
					}
					resourcesOnOtherSkillGroupsContainingThisPrimarySkill += resourcesOnOtherSkillGroup;
				}

				var otherSkillGroupOverstaff = Math.Max(resourcesOnOtherSkillGroupsContainingThisPrimarySkill - shovelResourceDataForInterval.FStaff, 0);
				dic.Add(primarySkill, primarySkillOverstaff - otherSkillGroupOverstaff);
			}

			if (primarySkillsExistsButTheyAreAllClosed)
			{
				var resources = skillGroupsWithSameIndex.Sum(x => x.RemainingResources);
				dic.Add(skillGroupsWithSameIndex.First().PrimarySkills.First(), resources);
			}

			return new ShovelResourcesState(dic,
				new ResourceDistributionForSkillGroupsWithSameIndex(skillGroupsWithSameIndex),
				!primarySkillsExistsButTheyAreAllClosed);
		}
	}
}