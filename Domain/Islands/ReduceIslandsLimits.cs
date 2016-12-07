using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Islands
{
	public class ReduceIslandsLimits
	{
		private readonly List<IslandLimits> _islandLimits;

		public ReduceIslandsLimits()
		{
			_islandLimits = new List<IslandLimits>();
			SetValues_UseOnlyFromTest(500, 4);
		}

		public int MinimumNumberOfAgentsInIsland => _islandLimits.First().MinAgentsInIsland;

		public int MinimumFactorOfAgentsInOtherSkillGroup(int numberOfAgentsInIsland)
		{
			var last = 0;
			foreach (var islandLimit in _islandLimits)
			{
				if (numberOfAgentsInIsland > islandLimit.MinAgentsInIsland)
				{
					last = islandLimit.MinFactorOfAgentsInOtherSkillGroup;
				}
				else
				{
					break;
				}
			}
			return last;
		}

		public void SetValues_UseOnlyFromTest(int minAgentsInIsland, int minFactorOfAgentsInOtherSkillGroup)
		{
			_islandLimits.Add(new IslandLimits(minAgentsInIsland, minFactorOfAgentsInOtherSkillGroup));
			_islandLimits.Sort();
		}
	}
}