using System;

namespace Teleopti.Ccc.Domain.Islands
{
	public class IslandLimits : IComparable<IslandLimits>
	{
		public IslandLimits(int minAgentsInIsland, double minFactorOfAgentsInOtherSkillGroup)
		{
			MinAgentsInIsland = minAgentsInIsland;
			MinFactorOfAgentsInOtherSkillGroup = minFactorOfAgentsInOtherSkillGroup;
		}

		public int MinAgentsInIsland { get; }
		public double MinFactorOfAgentsInOtherSkillGroup { get; }

		public int CompareTo(IslandLimits other)
		{
			return MinAgentsInIsland - other.MinAgentsInIsland;
		}
	}
}