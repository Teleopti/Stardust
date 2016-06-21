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
			var overstaffSum = 0d;
			var allPrimarySkillsClosed = true;

			foreach (var primarySkill in skillGroup.PrimarySkills)
			{
				var overStaff = skillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval, highValueForClosedSkill).AbsoluteDifference;
				if (skillIsClosed(overStaff))
					continue;

				allPrimarySkillsClosed = false;
				overstaffSum += overStaff;
			}

			if (allPrimarySkillsClosed)
			{
				overstaffSum = highValueForClosedSkill;
			}
			return overstaffSum;
		}

		private static bool skillIsClosed(double overStaff)
		{
			return Math.Abs(overStaff - highValueForClosedSkill) < 0.0000001;
		}
	}
}