using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public class SkillGroupIslandsAnalyzer
	{
		private readonly IList<Island> _islands = new List<Island>();

		public IList<Island> FindIslands(VirtualSkillGroupsCreatorResult skillGroups)
		{
			var keys = skillGroups.GetKeys();
			foreach (var key in keys)
			{
				var splittedSkillKeys = key.Split("|".ToCharArray());
				_islands.Add(new Island(splittedSkillKeys, new List<string>{key}));
			}

			var islandPair = checkIfJoinIslands();
			while (islandPair != null)
			{
				var joined = joinIslands(islandPair.Item1, islandPair.Item2);
				_islands.Remove(islandPair.Item1);
				_islands.Remove(islandPair.Item2);
				_islands.Add(joined);

				islandPair = checkIfJoinIslands();
			}

			return _islands;
		}

		private Tuple<Island, Island> checkIfJoinIslands()
		{
			foreach (var island in _islands)
			{
				foreach (var island1 in _islands)
				{
					if (island.Equals(island1))
						continue;

					foreach (var skillGuidString in island1.SkillGuidStrings)
					{
						if (island.SkillGuidStrings.Contains(skillGuidString))
						{
							return new Tuple<Island, Island>(island, island1);
						}
					}
				}
			}

			return null;
		}

		private static Island joinIslands(Island island1, Island island2)
		{
			var combinedGuidStrings = new List<string>(island1.SkillGuidStrings);
			combinedGuidStrings.AddRange(island2.SkillGuidStrings);

			var combinedGroupKeys = new List<string>(island1.GroupKeys);
			combinedGroupKeys.AddRange(island2.GroupKeys);

			return new Island(combinedGuidStrings, combinedGroupKeys);
		}

		public class Island
		{
			private readonly HashSet<string> _skillGuidStrings;
			private readonly IList<string> _groupKeys;

			public Island(IEnumerable<string> skillGuidStrings, IList<string> groupKeys)
			{
				_skillGuidStrings = new HashSet<string>(skillGuidStrings);
				_groupKeys = groupKeys;
			}

			public IList<string> SkillGuidStrings
			{
				get { return _skillGuidStrings.ToList(); }
			}

			public IList<string> GroupKeys
			{
				get { return _groupKeys; }
			}

			public IList<IPerson> PersonsInIsland(VirtualSkillGroupsCreatorResult skillGroupsCreatorResult)
			{
				var result = new List<IPerson>();
				foreach (var groupKey in _groupKeys)
				{
					result.AddRange(skillGroupsCreatorResult.GetPersonsForSkillGroupKey(groupKey));
				}

				return result;
			}
		}
	}
}