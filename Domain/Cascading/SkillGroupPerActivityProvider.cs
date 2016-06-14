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
			var affectedSkills = ResourceCalculationContext.Fetch().AffectedResources(activity, period).Values;
			//perf thingy - don't do this every time - look if necessary
			var cascadingSkills = _stateHolder().SchedulingResultState.CascadingSkills().Where(x => x.Activity.Equals(activity)).ToArray();
			var ret = new List<CascadingSkillGroup>();

			foreach (var skillGroup in affectedSkills)
			{
				var skillsUsedByPrimarySkill = cascadingSkills.Where(x => skillGroup.Skills.Contains(x)).ToArray();
				if (skillsUsedByPrimarySkill.Length > 1)
				{
					var primarySkill = skillsUsedByPrimarySkill.First();

					var orderedCascadingSkills = skillsUsedByPrimarySkill.Where(x => !x.Equals(primarySkill));
					var cascadingSkillGroupItems = new List<CascadingSkillGroupItem>();

					foreach (var orderedCascadingSkill in orderedCascadingSkills)
					{
						var last = cascadingSkillGroupItems.LastOrDefault();
						if (last == null || !orderedCascadingSkill.CascadingIndex.Value.Equals(last.CascadingIndex))
						{
							var cascadingSkillGroupItem = new CascadingSkillGroupItem();
							cascadingSkillGroupItem.AddSkill(orderedCascadingSkill);
							cascadingSkillGroupItems.Add(cascadingSkillGroupItem);
						}
						else
						{
							last.AddSkill(orderedCascadingSkill);
						}
					}
					ret.Add(new CascadingSkillGroup(primarySkill, cascadingSkillGroupItems, skillGroup.Resource));
				}
			}
			ret.Sort(new CascadingSkillGroupSorter());
			return ret;
		}
	}
}