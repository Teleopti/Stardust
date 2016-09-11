using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourcesToSubSkills
	{
		private const double minPrimaryOverstaffToContinue = 0.1;

		public void Execute(ShovelResourcesState shovelResourcesState, ISkillStaffPeriodHolder skillStaffPeriodHolder, CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			var maxToMoveForThisSkillGroup = shovelResourcesState.MaxToMoveForThisSkillGroup(skillGroup);
			var remaingResourcesForThisSkillGroup = maxToMoveForThisSkillGroup;
			var anySubSkillIsUnderstaffed = true;
			while (shovelResourcesState.RemainingOverstaffing > minPrimaryOverstaffToContinue &&
				remaingResourcesForThisSkillGroup > minPrimaryOverstaffToContinue &&
				anySubSkillIsUnderstaffed)
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
						var proportionalResourcesToMove = understaffingPercent/totalUnderstaffingPercent*remainingResourcesToShovel;

						var resourceToMove = Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove);
						shovelResourcesState.AddResourcesTo(skillStaffPeriodTo, resourceToMove);
						//TODO: fix this!
						remaingResourcesForThisSkillGroup -= resourceToMove;
						skillGroup.RemainingResources -= resourceToMove;
					}
				}
			}
		}
	}
}