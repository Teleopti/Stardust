using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Islands.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	public class AgentSkillReducer
	{
		private const int sizeFactor = 4;

		public IEnumerable<IPerson> ReduceOne(OldIsland island, VirtualSkillGroupsCreatorResult skillGroupsCreatorResult,
			IList<ISkill> loadedSkillList, DateOnly date, bool desc, IList<IPersonSkill> modifiedPersonSkills)
		{
			var orderedList = sortGroups(island, skillGroupsCreatorResult, desc);
			foreach (var keyCount in orderedList)
			{
				var selectedSkillKey = skillToRemove(skillGroupsCreatorResult, keyCount, loadedSkillList);
				if (selectedSkillKey != null)
				{
					var result = removeSkillFromGroup(skillGroupsCreatorResult, keyCount.Key, selectedSkillKey, loadedSkillList, date, modifiedPersonSkills);
					return result;
				}
			}

			return Enumerable.Empty<IPerson>();
		}

		private IEnumerable<IPerson> removeSkillFromGroup(VirtualSkillGroupsCreatorResult skillGroupsCreatorResult, string selectedGroupKey, string selectedSkillKey, IList<ISkill> loadedSkillList, DateOnly date, IList<IPersonSkill> modifiedPersonSkills)
		{
			var result = new List<IPerson>();
			foreach (var person in skillGroupsCreatorResult.GetPersonsForSkillGroupKey(selectedGroupKey))
			{
				Guid guid;
				if (!Guid.TryParse(selectedSkillKey, out guid))
					continue;

				var skill = loadedSkillList.FirstOrDefault(s => s.Id == guid);
				if (skill == null)
					continue;

				var period = person.Period(date);
				var personSkills = period.PersonSkillCollection.Where(ps => ps.Skill.Equals(skill)).ToList();
				if (personSkills.Any())
				{
					var personSkill = personSkills.First();
					((IPersonSkillModify) personSkill).Active = false;
					result.Add(person);
					modifiedPersonSkills.Add(personSkill);
				}
			}

			return result;
		}

		private string skillToRemove(VirtualSkillGroupsCreatorResult skillGroupsCreatorResult, keyCount item, IList<ISkill> loadedSkillList)
		{
			var maxCount = 0;
			string keyToRemove = null;
			var splitted = item.Key.Split("|".ToCharArray());
			if (splitted.Length == 1)
				return null;

			var activityCountDic = new Dictionary<IActivity, int>();
			foreach (var guidString in splitted)
			{
				Guid guid;
				if (!Guid.TryParse(guidString, out guid))
					continue;

				var skill = loadedSkillList.FirstOrDefault(s => s.Id == guid);
				if (skill == null)
					continue;

				int count;
				if (!activityCountDic.TryGetValue(skill.Activity, out count))
				{
					activityCountDic.Add(skill.Activity, 0);
				}

				activityCountDic[skill.Activity] ++;
			}

			foreach (var guidString in splitted)
			{
				Guid guid;
				if (!Guid.TryParse(guidString, out guid))
					continue;

				var skill = loadedSkillList.FirstOrDefault(s => s.Id == guid);
				if (skill == null)
					continue;

				if(activityCountDic[skill.Activity] < 2)
					continue;

				var agentCount = skillGroupsCreatorResult.GetPersonsForSkillKey(guidString).Count();
				if (agentCount > maxCount)
				{
					maxCount = agentCount;
					keyToRemove = guidString;
				}
			}

			if (item.Count*sizeFactor > maxCount)
				return null;

			return keyToRemove;
		}

		private IEnumerable<keyCount> sortGroups(OldIsland island, VirtualSkillGroupsCreatorResult skillGroupsCreatorResult, bool desc)
		{
			
			var keyCountList = new List<keyCount>();
			foreach (var key in skillGroupsCreatorResult.GetKeys())
			{
				if (!island.GroupKeys.Contains(key))
					continue;

				var count = skillGroupsCreatorResult.GetPersonsForSkillGroupKey(key).Count();
				keyCountList.Add(new keyCount {Key = key, Count = count});
			}

			if (desc)
				return keyCountList.OrderByDescending(x => x.Count);

			return keyCountList.OrderBy(x => x.Count);
		}

		class keyCount
		{
			public string Key { get; set; }
			public int Count { get; set; }
		}
	}
}