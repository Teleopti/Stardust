using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class SkillGroupPerActivityProvider
	{
		public OrderedSkillGroups FetchOrdered(CascadingSkills cascadingSkills, IResourcesForShovelAndCalculation resources, IActivity activity, DateTimePeriod period)
		{
			var affectedSkills = resources.AffectedResources(activity, period).Values;
			var cascadingSkillsForActivity = cascadingSkills.ForActivity(activity).ToArray();
			var cascadingSkillGroups = new List<CascadingSkillGroup>();

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
				cascadingSkillGroups.Add(new CascadingSkillGroup(primarySkills, cascadingSubSkills, skillGroup.Resource));
			}
			cascadingSkillGroups.Sort(new CascadingSkillGroupSorter());
			return mergeSkillGroupsWithSameIndex(cascadingSkillGroups);
		}

		private OrderedSkillGroups mergeSkillGroupsWithSameIndex(IEnumerable<CascadingSkillGroup> cascadingSkillGroups)
		{
			var ret = new List<List<CascadingSkillGroup>>();
			foreach (var skillGroup in cascadingSkillGroups)
			{
				var retLast = ret.LastOrDefault();
				if (retLast == null || !retLast.First().HasSameSkillGroupIndexAs(skillGroup))
				{
					ret.Add(new List<CascadingSkillGroup> {skillGroup});
				}
				else
				{
					ret.Last().Add(skillGroup);
				}
			}
			return new OrderedSkillGroups(ret);
		}
	}
}