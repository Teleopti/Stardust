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
			while (shovelResourcesState.RemainingOverstaffing > minPrimaryOverstaffToContinue && skillGroup.RemainingResources > minPrimaryOverstaffToContinue)
			{
				var anySubSkillIsUnderstaffed = false;

				foreach (var subSkillsWithSameIndex in skillGroup.SubSkillsWithSameIndex)
				{
					var totalUnderstaffingPercent = subSkillsWithSameIndex
						.Select(paralellSkill => skillStaffPeriodHolder.SkillStaffPeriodOrDefault(paralellSkill, interval))
						.Where(x => x.AbsoluteDifference.IsUnderstaffed())
						.Sum(x => -x.RelativeDifference);

					var remainingResourcesToShovel = shovelResourcesState.RemainingOverstaffing;

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
						resourceToMove = Math.Min(resourceToMove, shovelResourcesState.SkillgroupResourcesAtStart);
						shovelResourcesState.AddResourcesTo(skillStaffPeriodTo, resourceToMove);
					}
				}

				if (!anySubSkillIsUnderstaffed)
					return;
			}
		}
	}
}