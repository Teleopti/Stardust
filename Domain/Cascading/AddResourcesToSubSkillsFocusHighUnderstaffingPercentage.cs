using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourcesToSubSkillsFocusHighUnderstaffingPercentage
	{
		public void Execute(ShovelResourcesState shovelResourcesState, ISkillStaffPeriodHolder skillStaffPeriodHolder, CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			while (!shovelResourcesState.NoMoreResourcesToMove())
			{
				var subSkillIsUnderstaffed = false;

				foreach (var subSkillsWithSameIndex in skillGroup.SubSkillsWithSameIndex)
				{
					var totalUnderstaffingPercent = subSkillsWithSameIndex
						.Select(paralellSkill => skillStaffPeriodHolder.SkillStaffPeriodOrDefault(paralellSkill, interval, 0))
						.Where(x => x.AbsoluteDifference.IsUnderstaffed())
						.Sum(x => -x.RelativeDifference);

					var remainingOverstaff = shovelResourcesState.RemaingOverstaff();

					foreach (var skillToMoveTo in subSkillsWithSameIndex)
					{
						var skillStaffPeriodTo = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillToMoveTo, interval, 0);
						var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
						if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
							continue;

						subSkillIsUnderstaffed = true;
						var understaffingPercent = -skillStaffPeriodTo.RelativeDifference;
						var proportionalResourcesToMove = understaffingPercent/totalUnderstaffingPercent*remainingOverstaff;

						var resourceToMove = Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove);
						shovelResourcesState.AddResourcesTo(skillStaffPeriodTo, resourceToMove);
						if (shovelResourcesState.NoMoreResourcesToMove())
							return;
					}
				}

				if (!subSkillIsUnderstaffed)
					return;
			}
		}
	}
}