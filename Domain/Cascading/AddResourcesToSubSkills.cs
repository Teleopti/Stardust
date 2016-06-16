using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourcesToSubSkills
	{
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

		public AddResourcesToSubSkills(Func<ISchedulingResultStateHolder> schedulingResultStateHolder)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public double Execute(CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			var resourcesMoved = 0d;
			var schedulingResultStateHolder = _schedulingResultStateHolder();
			var remainingResourcesInGroup = skillGroup.Resources;
			var remainingPrimarySkillOverstaff = skillGroup.PrimarySkills
				.Sum(primarySkill => schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval, int.MaxValue).AbsoluteDifference);
			if (!remainingPrimarySkillOverstaff.IsOverstaffed())
				return 0;

			foreach (var cascadingSkillGroupItem in skillGroup.CascadingSkillGroupItems)
			{
				var totalUnderstaffingInSkillGroup = cascadingSkillGroupItem.SubSkills
					.Select(skillToMoveTo => schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillToMoveTo, interval, 0).AbsoluteDifference)
					.Where(absoluteDifference => absoluteDifference.IsUnderstaffed())
					.Sum(absoluteDifference => -absoluteDifference);

				var remainingOverstaff = Math.Min(remainingPrimarySkillOverstaff, remainingResourcesInGroup);
				foreach (var skillToMoveTo in cascadingSkillGroupItem.SubSkills)
				{
					var skillStaffPeriodTo = schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillToMoveTo, interval, 0);
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