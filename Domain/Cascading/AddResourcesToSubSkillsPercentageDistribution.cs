using static System.Math;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourcesToSubSkillsPercentageDistribution
	{
		public void Execute(ShovelResourcesState shovelResourcesState, ISkillStaffPeriodHolder skillStaffPeriodHolder, CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			if (shovelResourcesState.NoMoreResourcesToMove())
				return;

			foreach (var subSkillsWithSameIndex in skillGroup.SubSkillsWithSameIndex)
			{
				var totalUnderstaffingForSkillsWithSameIndex = subSkillsWithSameIndex
					.Select(skillToMoveTo => skillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillToMoveTo, interval, 0).AbsoluteDifference)
					.Where(absoluteDifference => absoluteDifference.IsUnderstaffed())
					.Sum(absoluteDifference => -absoluteDifference);

				var remainingOverstaff = shovelResourcesState.RemaingOverstaff();
				foreach (var skillToMoveTo in subSkillsWithSameIndex)
				{
					var skillStaffPeriodTo = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillToMoveTo, interval, 0);
					var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
					if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
						continue;

					var proportionalResourcesToMove = -skillToMoveToAbsoluteDifference / totalUnderstaffingForSkillsWithSameIndex * remainingOverstaff;
					var resourceToMove = Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove);

					shovelResourcesState.AddResourcesTo(skillStaffPeriodTo, resourceToMove);
					if (shovelResourcesState.NoMoreResourcesToMove())
						return;
				}
			}
		}
	}
}