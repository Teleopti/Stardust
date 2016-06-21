using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourcesToSubSkills
	{
		public double Execute(ISkillStaffPeriodHolder skillStaffPeriodHolder, CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			var resourcesMoved = 0d;
			var remainingResourcesInGroup = skillGroup.Resources;
			double remainingPrimarySkillOverstaff = 0;
			bool allPrimarySkillsClosed = true;

			foreach (var primarySkill in skillGroup.PrimarySkills)
			{
				var overStaff = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval, DoubleExtensionsInCascadingScenarios.ValueForClosedSkill).AbsoluteDifference;

				if (!overStaff.IsClosed())
				{
					allPrimarySkillsClosed = false;
					remainingPrimarySkillOverstaff += overStaff;
				}		
			}

			if (allPrimarySkillsClosed)
				remainingPrimarySkillOverstaff = int.MaxValue;

			if (!remainingPrimarySkillOverstaff.IsOverstaffed())
				return 0;

			foreach (var cascadingSkillGroupItem in skillGroup.CascadingSkillGroupItems)
			{
				var totalUnderstaffingInSkillGroup = cascadingSkillGroupItem.SubSkills
					.Select(skillToMoveTo => skillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillToMoveTo, interval, 0).AbsoluteDifference)
					.Where(absoluteDifference => absoluteDifference.IsUnderstaffed())
					.Sum(absoluteDifference => -absoluteDifference);

				var remainingOverstaff = Math.Min(remainingPrimarySkillOverstaff, remainingResourcesInGroup);
				foreach (var skillToMoveTo in cascadingSkillGroupItem.SubSkills)
				{
					var skillStaffPeriodTo = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillToMoveTo, interval, 0);
					var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
					if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
						continue;

					var proportionalResourcesToMove = -skillToMoveToAbsoluteDifference / totalUnderstaffingInSkillGroup * remainingOverstaff;
					var resourceToMove = Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove);

					skillStaffPeriodTo.AddResources(resourceToMove);
					remainingResourcesInGroup -= resourceToMove;
					remainingPrimarySkillOverstaff -= resourceToMove;
					resourcesMoved += resourceToMove;
					if (remainingPrimarySkillOverstaff.IsZero() || remainingResourcesInGroup.IsZero())
						return resourcesMoved;
				}
			}
			return resourcesMoved;
		}
	}
}