using System.Collections.Generic;
using System.Linq;

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

		public IList<SkillGroupReducerResult> SuggestAction(VirtualSkillGroupsCreatorResult skillGroups)
		{
			var results = new List<SkillGroupReducerResult>();
			var keyList = new List<string>(skillGroups.GetKeys());
			string maxAgentGroup = string.Empty;
			int maxAgents = int.MinValue;
			foreach (var key in keyList)
			{
				var numAgents = skillGroups.GetPersonsForKey(key).Count();
				if (numAgents > maxAgents)
				{
					maxAgentGroup = key;
					maxAgents = numAgents;
				}
			}
			results.AddRange(tryFind(keyList, skillGroups, maxAgentGroup));
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

				if(maxAgents<10)
					break;

				results.AddRange(tryFind(keyList, skillGroups, maxAgentGroup));
			}

			return results;
		}

		private static IList<SkillGroupReducerResult> tryFind(IEnumerable<string> keyList,
			VirtualSkillGroupsCreatorResult skillGroups, string maxAgentGroup)
		{
			var results = new List<SkillGroupReducerResult>();
			var minAgentList = new List<string>();
			foreach (var key in keyList)
			{
				var numAgents = skillGroups.GetPersonsForKey(key).Count();

				if (numAgents == 1)
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
					var result = new SkillGroupReducerResult {RemoveFromGroupKey = key, SkillGuidStringToRemove = common.First(), Releasing = maxAgentGroup};
					results.Add(result);
				}
			}

			return results;
		}

	}
}