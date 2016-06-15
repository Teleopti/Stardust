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
			var cascadingSkillsForActivity = _stateHolder().SchedulingResultState.CascadingSkills().Where(x => x.Activity.Equals(activity)).ToArray();
			var ret = new List<CascadingSkillGroup>();

			foreach (var skillGroup in affectedSkills)
			{
				var cascadingSkillsInSkillGroup = cascadingSkillsForActivity.Where(x => skillGroup.Skills.Contains(x)).ToArray();
				if (cascadingSkillsInSkillGroup.Length <= 1)
					continue;

				var lowestCascadingIndex = cascadingSkillsInSkillGroup.Min(x => x.CascadingIndex.Value);
				var primarySkills = cascadingSkillsInSkillGroup.Where(x => x.CascadingIndex.Value==lowestCascadingIndex);
				var cascadingSkillGroupItems = new List<CascadingSkillGroupItem>();
				foreach (var skillInSameChainAsPrimarySkill in cascadingSkillsInSkillGroup.Where(x => !primarySkills.Contains(x)))
				{
					var last = cascadingSkillGroupItems.LastOrDefault();
					if (last == null || !skillInSameChainAsPrimarySkill.CascadingIndex.Value.Equals(last.CascadingIndex))
					{
						var cascadingSkillGroupItem = new CascadingSkillGroupItem();
						cascadingSkillGroupItem.AddSkill(skillInSameChainAsPrimarySkill);
						cascadingSkillGroupItems.Add(cascadingSkillGroupItem);
					}
					else
					{
						last.AddSkill(skillInSameChainAsPrimarySkill);
					}
				}
				ret.Add(new CascadingSkillGroup(primarySkills, cascadingSkillGroupItems, skillGroup.Resource));
			}
			ret.Sort(new CascadingSkillGroupSorter());
			return ret;
		}
	}
}