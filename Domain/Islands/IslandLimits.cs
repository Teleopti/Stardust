using System;

namespace Teleopti.Ccc.Domain.Islands
{
	public class IslandLimits : IComparable<IslandLimits>
	{
		public IslandLimits(int minAgentsInIsland, int minFactorOfAgentsInOtherSkillGroup)
		{
			MinAgentsInIsland = minAgentsInIsland;
			MinFactorOfAgentsInOtherSkillGroup = minFactorOfAgentsInOtherSkillGroup;
		}

		public int MinAgentsInIsland { get; private set; }
		public int MinFactorOfAgentsInOtherSkillGroup { get; private set; }

		public int CompareTo(IslandLimits other)
		{
			return MinAgentsInIsland - other.MinAgentsInIsland;
		}
	}
}