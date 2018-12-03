using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class PrimarySkillOverstaff
	{
		public ShovelResourcesState AvailableSum(IShovelResourceData shovelResourceData, 
			IEnumerable<CascadingSkillSet> allSkillSets, 
			IEnumerable<CascadingSkillSet> skillSetsWithSameIndex, 
			DateTimePeriod interval)
		{
			var primarySkillsExistsButTheyAreAllClosed = true;
			var dic = new Dictionary<ISkill, double>();

			foreach (var primarySkill in skillSetsWithSameIndex.First().PrimarySkills)
			{
				if (!shovelResourceData.TryGetDataForInterval(primarySkill, interval, out var shovelResourceDataForInterval))
					continue;
				primarySkillsExistsButTheyAreAllClosed = false;
				var primarySkillOverstaff = shovelResourceDataForInterval.AbsoluteDifference;
				if (!primarySkillOverstaff.IsOverstaffed())
					continue;

				var resourcesOnOtherSkillSetsContainingThisPrimarySkill = 0d;
				foreach (var otherSkillSet in allSkillSets.Where(x => !skillSetsWithSameIndex.Contains(x) && x.PrimarySkills.Contains(primarySkill)))
				{
					var resourcesOnOtherSkillSet = otherSkillSet.RemainingResources;
					foreach (var otherPrimarySkill in otherSkillSet.PrimarySkills.Where(x => !x.Equals(primarySkill)))
					{
						resourcesOnOtherSkillSet -= shovelResourceData.GetDataForInterval(otherPrimarySkill, interval).CalculatedResource;
					}
					resourcesOnOtherSkillSetsContainingThisPrimarySkill += resourcesOnOtherSkillSet;
				}

				var otherSkillSetOverstaff = Math.Max(resourcesOnOtherSkillSetsContainingThisPrimarySkill - shovelResourceDataForInterval.FStaff, 0);
				dic.Add(primarySkill, primarySkillOverstaff - otherSkillSetOverstaff);
			}

			if (primarySkillsExistsButTheyAreAllClosed)
			{
				var resources = skillSetsWithSameIndex.Sum(x => x.RemainingResources);
				dic.Add(skillSetsWithSameIndex.First().PrimarySkills.First(), resources);
			}

			return new ShovelResourcesState(dic,
				new ResourceDistributionForSkillSetsWithSameIndex(skillSetsWithSameIndex),
				!primarySkillsExistsButTheyAreAllClosed);
		}
	}
}