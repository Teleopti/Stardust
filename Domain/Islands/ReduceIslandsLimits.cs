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
			addValues(500, 4);
			addValues(3000, 1.5);
		}

		public int MinimumNumberOfAgentsInIsland => _islandLimits.First().MinAgentsInIsland;

		public double MinimumFactorOfAgentsInOtherSkillSet(int numberOfAgentsInIsland)
		{
			var last = 0d;
			foreach (var islandLimit in _islandLimits)
			{
				if (numberOfAgentsInIsland > islandLimit.MinAgentsInIsland)
				{
					last = islandLimit.MinFactorOfAgentsInOtherSkillSet;
				}
				else
				{
					break;
				}
			}
			return last;
		}

		private void addValues(int minAgentsInIsland, double minFactorOfAgentsInOtherSkillSet)
		{
			_islandLimits.Add(new IslandLimits(minAgentsInIsland, minFactorOfAgentsInOtherSkillSet));
			_islandLimits.Sort();
		}

		public void SetValues_UseOnlyFromTest(int minAgentsInIsland, int minFactorOfAgentsInOtherSkillSet)
		{
			addValues(minAgentsInIsland, minFactorOfAgentsInOtherSkillSet);
		}
	}
}