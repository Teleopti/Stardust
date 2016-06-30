using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class PrimarySkillOverstaff
	{
		private const int highValueForClosedSkill = int.MaxValue;

		public double Sum(ISkillStaffPeriodHolder skillStaffPeriodHolder, CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			var overstaffingOnOverstaffedSkills = 0d;
			var resourcesOnSkillsOnCurrentSkillGroup = 0d;
			var allPrimarySkillsClosed = true;
			var forcastedForOverstaffedSkills = 0d;

			foreach (var primarySkill in skillGroup.PrimarySkills)
			{
				ISkillStaffPeriod skillStaffPeriod;
				if (!skillStaffPeriodHolder.TryGetSkillStaffPeriod(primarySkill, interval, out skillStaffPeriod))
					continue;

				var absDiff = skillStaffPeriod.AbsoluteDifference;
				resourcesOnSkillsOnCurrentSkillGroup += skillStaffPeriod.CalculatedResource;
				if (absDiff.IsOverstaffed())
				{
					forcastedForOverstaffedSkills += skillStaffPeriod.FStaff;
					overstaffingOnOverstaffedSkills += absDiff;
				}
				allPrimarySkillsClosed = false;
			}

			if (allPrimarySkillsClosed)
			{
				return highValueForClosedSkill;
			}
			var resourcesOnSkillsComingFromOtherSkillGroups = resourcesOnSkillsOnCurrentSkillGroup - skillGroup.Resources;
			if (resourcesOnSkillsComingFromOtherSkillGroups > overstaffingOnOverstaffedSkills)
			{
				return overstaffingOnOverstaffedSkills;
			}

			var overstaffingToBeKeptForOtherSkillGroups = resourcesOnSkillsComingFromOtherSkillGroups - forcastedForOverstaffedSkills;
			return overstaffingToBeKeptForOtherSkillGroups > 0
				? overstaffingOnOverstaffedSkills - overstaffingToBeKeptForOtherSkillGroups
				: overstaffingOnOverstaffedSkills;
		}
	}
}