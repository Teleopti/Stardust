using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Islands
{
	public class ReduceIslandsLimits
	{
		public ReduceIslandsLimits()
		{
			_values = new List<IslandLimits>();
			SetValues_UseOnlyFromTest(500, 4);
		}

		public int MinimumNumberOfAgentsInIsland => _values.First().MinAgentsInIsland;

		public int MinimumFactorOfAgentsInOtherSkillGroup => _values.First().MinFactorOfAgentsInOtherSkillGroup;

		private readonly List<IslandLimits> _values;

		//TODO: byt namn
		public void SetValues_UseOnlyFromTest(int minAgentsInIsland, int minFactorOfAgentsInOtherSkillGroup)
		{
			_values.Add(new IslandLimits(minAgentsInIsland, minFactorOfAgentsInOtherSkillGroup));
			_values.Sort();
		}
	}
}