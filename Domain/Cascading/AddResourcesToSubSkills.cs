using static System.Math;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourcesToSubSkills
	{
		private readonly PrimarySkillOverstaff _primarySkillOverstaff;

		public AddResourcesToSubSkills(PrimarySkillOverstaff primarySkillOverstaff)
		{
			_primarySkillOverstaff = primarySkillOverstaff;
		}

		public double Execute(ISkillStaffPeriodHolder skillStaffPeriodHolder, CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			var primarySkillOverstaff = _primarySkillOverstaff.Sum(skillStaffPeriodHolder, skillGroup, interval);
			if (!primarySkillOverstaff.IsOverstaffed())
				return 0;
			var shovelResourcesState = new ShovelResourcesState(skillGroup.Resources, primarySkillOverstaff);

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
						return shovelResourcesState.ResourcesMoved;
				}
			}
			return shovelResourcesState.ResourcesMoved;
		}
	}
}