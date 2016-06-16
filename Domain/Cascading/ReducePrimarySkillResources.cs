using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ReducePrimarySkillResources
	{
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

		public ReducePrimarySkillResources(Func<ISchedulingResultStateHolder> schedulingResultStateHolder)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public void Execute(IEnumerable<ISkill> primarySkills, DateTimePeriod interval, double resourcesMoved)
		{
			var schedulingResultStateHolder = _schedulingResultStateHolder();
			var primarySkillOverstaff = primarySkills
				.Sum(primarySkill => schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval, 0).AbsoluteDifference);
			if (primarySkillOverstaff.IsZero())
				return;

			foreach (var primarySkill in primarySkills)
			{
				var skillStaffPeriod = schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval, 0);
				var overstaffForSkill = skillStaffPeriod.AbsoluteDifference;
				var percentageOverstaff = overstaffForSkill / primarySkillOverstaff;
				var resourceToSubtract = resourcesMoved * percentageOverstaff;
				skillStaffPeriod.AddResources(-resourceToSubtract);
			}
		}
	}
}