using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class SkillGroupPerActivityProvider
	{
		private readonly Func<ISchedulerStateHolder> _stateHolder;

		public SkillGroupPerActivityProvider(Func<ISchedulerStateHolder> stateHolder)
		{
			_stateHolder = stateHolder;
		}

		public IEnumerable<CascadingSkillGroup> FetchOrdered(IActivity activity, DateTimePeriod period)
		{
			//TODO: it seems that AffectedResources gives back skillgroups with skills using different activities -> incorrect resources below probably...
			var affectedSkills = ResourceCalculationContext.Fetch().AffectedResources(activity, period).Values;
			//perf thingy - don't do this every time - look if necessary
			var cascadingSkills = _stateHolder().SchedulingResultState.CascadingSkills().Where(x => x.Activity.Equals(activity)).ToArray();

			var ret = (from skillGroup in affectedSkills
								 let skillsUsedByPrimarySkill = cascadingSkills.Where(x => skillGroup.Skills.Contains(x)).ToArray()
								 where skillsUsedByPrimarySkill.Length > 1
								 let primarySkill = skillsUsedByPrimarySkill.First()
								 select new CascadingSkillGroup(primarySkill, skillsUsedByPrimarySkill.Where(x => !x.Equals(primarySkill)), skillGroup.Resource)).ToList();
			ret.Sort(new CascadingSkillGroupSorter());
			return ret;
		}
	}
}