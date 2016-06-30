using System;
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
				var skillStaffPeriod = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval, highValueForClosedSkill);
				var absDiff = skillStaffPeriod.AbsoluteDifference;
				
				if (skillIsClosed(absDiff))
					continue;

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

		private static bool skillIsClosed(double overStaff)
		{
			return Math.Abs(overStaff - highValueForClosedSkill) < 0.0000001;
		}
	}
}