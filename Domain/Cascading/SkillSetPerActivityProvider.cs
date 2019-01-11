using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class SkillSetPerActivityProvider
	{
		public OrderedSkillSets FetchOrdered(CascadingSkills cascadingSkills, IResourceCalculationDataContainer resources, IActivity activity, DateTimePeriod period)
		{
			var affectedSkills = resources.AffectedResources(activity, period).Values;
			var cascadingSkillsForActivity = cascadingSkills.ForActivity(activity).ToArray();
			var cascadingSkillSets = new List<CascadingSkillSet>();

			foreach (var skillSet in affectedSkills)
			{
				var cascadingSkillsInSkillSet = cascadingSkillsForActivity.Intersect(skillSet.Skills).ToArray();
				if(!cascadingSkillsInSkillSet.Any())
					continue;

				var lowestCascadingIndex = cascadingSkillsInSkillSet.Min(x => x.CascadingIndex.Value);
				var primarySkills = cascadingSkillsInSkillSet.Where(x => x.CascadingIndex.Value==lowestCascadingIndex).ToArray();
				var cascadingSubSkills = new List<SubSkillsWithSameIndex>();
				foreach (var skillInSameChainAsPrimarySkill in cascadingSkillsInSkillSet.Except(primarySkills))
				{
					var last = cascadingSubSkills.LastOrDefault();
					if (last == null || !skillInSameChainAsPrimarySkill.CascadingIndex.Value.Equals(last.CascadingIndex))
					{
						var cascadingSkillSetItem = new SubSkillsWithSameIndex();
						cascadingSkillSetItem.AddSubSkill(skillInSameChainAsPrimarySkill);
						cascadingSubSkills.Add(cascadingSkillSetItem);
					}
					else
					{
						last.AddSubSkill(skillInSameChainAsPrimarySkill);
					}
				}				
				addOrAppendSkillSet(cascadingSkillSets, primarySkills, cascadingSubSkills, skillSet.Resource);
			}
			cascadingSkillSets.Sort(new CascadingSkillSetSorter());
			return mergeSkillSetsWithSameIndex(cascadingSkillSets);
		}

		private static void addOrAppendSkillSet(ICollection<CascadingSkillSet> cascadingSkillSets, IEnumerable<ISkill> primarySkills, IEnumerable<SubSkillsWithSameIndex> cascadingSubSkills, double skillSetResource)
		{
			var presentSkillSet = cascadingSkillSets.SingleOrDefault(x => x.ContainsSameSkills(primarySkills, cascadingSubSkills));
			if (presentSkillSet == null)
			{
				cascadingSkillSets.Add(new CascadingSkillSet(primarySkills, cascadingSubSkills, skillSetResource));
			}
			else
			{
				//we have duplicate cascading skillsets if there were non cascading skills in the set
				presentSkillSet.RemainingResources += skillSetResource;
			}
		}
		
		private static OrderedSkillSets mergeSkillSetsWithSameIndex(IEnumerable<CascadingSkillSet> cascadingSkillSets)
		{
			var ret = new List<List<CascadingSkillSet>>();
			foreach (var skillSet in cascadingSkillSets)
			{
				var retLast = ret.LastOrDefault();
				if (retLast == null || !retLast.First().HasSameSkillSetIndexAs(skillSet))
				{
					ret.Add(new List<CascadingSkillSet> {skillSet});
				}
				else
				{
					ret.Last().Add(skillSet);
				}
			}
			return new OrderedSkillSets(ret);
		}
	}
}