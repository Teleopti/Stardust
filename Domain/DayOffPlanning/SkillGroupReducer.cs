﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public class SkillGroupReducer
	{
		public class SkillGroupReducerResult
		{
			public string SkillGuidStringToRemove { get; set; }
			public string RemoveFromGroupKey { get; set; }
			public string Releasing { get; set; }
		}

		public IList<SkillGroupReducerResult> SuggestAction(VirtualSkillGroupsCreatorResult skillGroups, IList<SkillGroupIslandsAnalyzer.Island> islands, IList<ISkill> allSkillList)
		{
			var results = new List<SkillGroupReducerResult>();
			foreach (var island in islands)
			{
				IList<string> keyList = island.GroupKeys;
				results.AddRange(runOneIsland(keyList, skillGroups, allSkillList));
			}

			return results;
		}

		private IEnumerable<SkillGroupReducerResult> runOneIsland(IList<string> keyList, VirtualSkillGroupsCreatorResult skillGroups, IList<ISkill> allSkillList)
		{
			var results = new List<SkillGroupReducerResult>();
			string maxAgentGroup = string.Empty;
			int maxAgents = int.MinValue;
			var totalAgentsInIsland = 0;
			foreach (var key in keyList)
			{
				var numAgents = skillGroups.GetPersonsForKey(key).Count();
				totalAgentsInIsland += numAgents;
				if (numAgents > maxAgents)
				{
					maxAgentGroup = key;
					maxAgents = numAgents;
				}
			}
			results.AddRange(tryFind(keyList, skillGroups, maxAgentGroup, (int)Math.Round(totalAgentsInIsland * 0.02, 0), allSkillList));
			while (true)
			{
				keyList.Remove(maxAgentGroup);
				maxAgentGroup = string.Empty;
				maxAgents = int.MinValue;
				foreach (var key in keyList)
				{
					var numAgents = skillGroups.GetPersonsForKey(key).Count();
					if (numAgents > maxAgents)
					{
						maxAgentGroup = key;
						maxAgents = numAgents;
					}
				}

				var maxAgentLimit = Math.Round(totalAgentsInIsland*0.01, 0);
				if (maxAgentLimit < 10)
					maxAgentLimit = 10;

				if (maxAgents < maxAgentLimit)
					break;

				results.AddRange(tryFind(keyList, skillGroups, maxAgentGroup, (int)Math.Round(totalAgentsInIsland * 0.02, 0), allSkillList));
			}

			return results;
		}

		private static IEnumerable<SkillGroupReducerResult> tryFind(IEnumerable<string> keyList,
			VirtualSkillGroupsCreatorResult skillGroups, string maxAgentGroup, int numAgentLimit, IList<ISkill> allSkillList)
		{
			if (numAgentLimit < 1)
				numAgentLimit = 1;

			var results = new List<SkillGroupReducerResult>();
			var minAgentList = new List<string>();
			foreach (var key in keyList)
			{
				var numAgents = skillGroups.GetPersonsForKey(key).Count();

				if (numAgents <= numAgentLimit)
					minAgentList.Add(key);
			}

			var maxAgentGroupSkills = maxAgentGroup.Split("|".ToCharArray());
			foreach (var key in minAgentList)
			{
				var splitted = key.Split("|".ToCharArray());
				if(splitted.Count() < 2)
					continue;

				var common = maxAgentGroupSkills.Where(x => splitted.Contains(x)).ToList();
				if (common.Count() == 1)
				{
					if(activityCheckBeforeRemove(key,common.First(), allSkillList))
					{
						var result = new SkillGroupReducerResult {RemoveFromGroupKey = key, SkillGuidStringToRemove = common.First(), Releasing = maxAgentGroup};
						results.Add(result);
					}
				}
			}

			return results;
		}

		private static bool activityCheckBeforeRemove(string removeFromGroupKey, string skillGuidStringToRemove, IList<ISkill> allSkillList)
		{
			var activitiesInGroup = new Dictionary<IActivity, int>();
			var splitted = removeFromGroupKey.Split("|".ToCharArray());
			IActivity activityToRemove = null;
			foreach (var guidString in splitted)
			{
				Guid guid;
				if (!Guid.TryParse(guidString, out guid))
					continue;

				var skill = allSkillList.FirstOrDefault(s => s.Id == guid);

				if (skill == null)
					continue;
			
				if (guidString == skillGuidStringToRemove)
					activityToRemove = skill.Activity;

				if(!activitiesInGroup.ContainsKey(skill.Activity))
					activitiesInGroup.Add(skill.Activity, 1);
				else
					activitiesInGroup[skill.Activity] ++;
				
			}

			return activityToRemove != null && activitiesInGroup[activityToRemove] > 1;
		}
	}
}