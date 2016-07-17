using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class SkillGroupPerActivityProvider
	{
		public IEnumerable<CascadingSkillGroup> FetchOrdered(CascadingSkills cascadingSkills, IActivity activity, DateTimePeriod period)
		{
			var affectedSkills = ResourceCalculationContext.Fetch().AffectedResources(activity, period).Values;
			var cascadingSkillsForActivity = cascadingSkills.ForActivity(activity).ToArray();
			var ret = new List<CascadingSkillGroup>();

			foreach (var skillGroup in affectedSkills)
			{
				var cascadingSkillsInSkillGroup = cascadingSkillsForActivity.Where(x => skillGroup.Skills.Contains(x)).ToArray();
				if(!cascadingSkillsInSkillGroup.Any())
					continue;

				var lowestCascadingIndex = cascadingSkillsInSkillGroup.Min(x => x.CascadingIndex.Value);
				var primarySkills = cascadingSkillsInSkillGroup.Where(x => x.CascadingIndex.Value==lowestCascadingIndex);
				var cascadingSubSkills = new List<SubSkillsWithSameIndex>();
				foreach (var skillInSameChainAsPrimarySkill in cascadingSkillsInSkillGroup.Where(x => !primarySkills.Contains(x)))
				{
					var last = cascadingSubSkills.LastOrDefault();
					if (last == null || !skillInSameChainAsPrimarySkill.CascadingIndex.Value.Equals(last.CascadingIndex))
					{
						var cascadingSkillGroupItem = new SubSkillsWithSameIndex();
						cascadingSkillGroupItem.AddSubSkill(skillInSameChainAsPrimarySkill);
						cascadingSubSkills.Add(cascadingSkillGroupItem);
					}
					else
					{
						last.AddSubSkill(skillInSameChainAsPrimarySkill);
					}
				}
				ret.Add(new CascadingSkillGroup(primarySkills, cascadingSubSkills, skillGroup.Resource));
			}
			ret.Sort(new CascadingSkillGroupSorter());
			return ret;
		}
	}
}