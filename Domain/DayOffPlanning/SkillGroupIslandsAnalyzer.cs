using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public class SkillGroupIslandsAnalyzer
	{
		public IList<Island> FindIslands(VirtualSkillGroupsCreatorResult skillGroups)
		{
			var islands = new List<Island>();
			var keys = skillGroups.GetKeys();
			foreach (var key in keys)
			{
				var splittedSkillKeys = key.Split("|".ToCharArray());
				islands.Add(new Island(splittedSkillKeys, new List<string> {key}, skillGroups));
			}

			var islandPair = checkIfJoinIslands(islands);
			while (islandPair != null)
			{
				var joined = joinIslands(islandPair.Item1, islandPair.Item2, skillGroups);
				islands.Remove(islandPair.Item1);
				islands.Remove(islandPair.Item2);
				islands.Add(joined);

				islandPair = checkIfJoinIslands(islands);
			}

			return islands;
		}

		private static Tuple<Island, Island> checkIfJoinIslands(IEnumerable<Island> islands)
		{
			foreach (var island in islands)
			{
				foreach (var island1 in islands)
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

		private static Island joinIslands(Island island1, Island island2,
			VirtualSkillGroupsCreatorResult skillGroupsCreatorResult)
		{
			var combinedGuidStrings = new List<string>(island1.SkillGuidStrings);
			combinedGuidStrings.AddRange(island2.SkillGuidStrings);

			var combinedGroupKeys = new List<string>(island1.GroupKeys);
			combinedGroupKeys.AddRange(island2.GroupKeys);

			return new Island(combinedGuidStrings, combinedGroupKeys, skillGroupsCreatorResult);
		}
	}

	public class Island
		{
			private readonly HashSet<string> _skillGuidStrings;
			private readonly IList<string> _groupKeys;
			private readonly VirtualSkillGroupsCreatorResult _skillGroupsCreatorResult;

			public Island(IEnumerable<string> skillGuidStrings, IList<string> groupKeys, VirtualSkillGroupsCreatorResult skillGroupsCreatorResult)
			{
				_skillGuidStrings = new HashSet<string>(skillGuidStrings);
				_groupKeys = groupKeys;
				_skillGroupsCreatorResult = skillGroupsCreatorResult;
			}

			public IList<string> SkillGuidStrings
			{
				get { return _skillGuidStrings.ToList(); }
			}

			public IList<string> GroupKeys
			{
				get { return _groupKeys; }
			}

			public IList<IPerson> PersonsInIsland()
			{
				var result = new List<IPerson>();
				foreach (var groupKey in _groupKeys)
				{
					result.AddRange(_skillGroupsCreatorResult.GetPersonsForSkillGroupKey(groupKey));
				}

				return result;
			}
		}
	}
