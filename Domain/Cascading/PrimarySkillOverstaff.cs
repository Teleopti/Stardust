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
			//TODO: fix all break-foreach and skillgroup.first() due to only interested in prim skills
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
				//TODO: does skillGroupsWithSameIndex need to be ALL skills?
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
				//behöver man ta hänsyn till skillgrupper med samma index även här?
				foreach (var skillGroup in skillGroupsWithSameIndex)
				{
					dic.Add(skillGroup.PrimarySkills.First(), skillGroup.RemainingResources);
					break;
				}
			}

			return new ShovelResourcesState(dic, foo(skillStaffPeriodHolder, skillGroupsWithSameIndex, interval)); 
		}

		private IDictionary<CascadingSkillGroup, double> foo(ISkillStaffPeriodHolder skillStaffPeriodHolder, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval)
		{
			var ret = new Dictionary<CascadingSkillGroup, double>();
			var tottiRelativeDifference = 0d;
			foreach (var skillGroupWithSameIndex in skillGroupsWithSameIndex)
			{
				foreach (var otherPrimarySkill in skillGroupWithSameIndex.PrimarySkills)
				{
					var relDiffInOtherPrimarySkill = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(otherPrimarySkill, interval).AbsoluteDifference;
					if (!double.IsNaN(relDiffInOtherPrimarySkill)) //TODO: Check for positive value? Needed here?
					{
						tottiRelativeDifference += relDiffInOtherPrimarySkill;
					}
				}
			}
			foreach (var skillGroup in skillGroupsWithSameIndex)
			{
				var myrelativeDifference = skillGroup.PrimarySkills.Sum(primarySkill => skillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval).AbsoluteDifference);
				var myFactor = myrelativeDifference/tottiRelativeDifference;
				ret[skillGroup] = double.IsNaN(myFactor) ? 1 : myFactor;
			}
			return ret;
		}
	}
}