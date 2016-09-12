using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourcesToSubSkills
	{
		public void Execute(ShovelResourcesState shovelResourcesState, ISkillStaffPeriodHolder skillStaffPeriodHolder, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval)
		{
			foreach (var skillGroup in skillGroupsWithSameIndex)
			{
				doForSkillgroup(shovelResourcesState, skillStaffPeriodHolder, skillGroup, interval);
			}
		}

		private static void doForSkillgroup(ShovelResourcesState shovelResourcesState, ISkillStaffPeriodHolder skillStaffPeriodHolder, CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			var maxToMoveForThisSkillGroup = shovelResourcesState.MaxToMoveForThisSkillGroup(skillGroup);
			var anySubSkillIsUnderstaffed = true;
			while (shovelResourcesState.ContinueShovel(skillGroup) && anySubSkillIsUnderstaffed)
			{
				anySubSkillIsUnderstaffed = false;

				foreach (var subSkillsWithSameIndex in skillGroup.SubSkillsWithSameIndex)
				{
					var totalUnderstaffingPercent = subSkillsWithSameIndex
						.Select(paralellSkill => skillStaffPeriodHolder.SkillStaffPeriodOrDefault(paralellSkill, interval))
						.Where(x => x.AbsoluteDifference.IsUnderstaffed())
						.Sum(x => -x.RelativeDifference);

					var remainingResourcesToShovel = Math.Min(shovelResourcesState.RemainingOverstaffing, maxToMoveForThisSkillGroup);

					foreach (var skillToMoveTo in subSkillsWithSameIndex)
					{
						var skillStaffPeriodTo = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillToMoveTo, interval);
						var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
						if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
							continue;

						anySubSkillIsUnderstaffed = true;
						var understaffingPercent = -skillStaffPeriodTo.RelativeDifference;
						var proportionalResourcesToMove = understaffingPercent / totalUnderstaffingPercent * remainingResourcesToShovel;

						var resourceToMove = Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove);
						shovelResourcesState.AddResourcesTo(skillStaffPeriodTo, skillGroup, resourceToMove);
					}
				}
			}
		}
	}
}