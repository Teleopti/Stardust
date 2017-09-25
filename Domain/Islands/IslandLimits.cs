using System;

namespace Teleopti.Ccc.Domain.Islands
{
	public class IslandLimits : IComparable<IslandLimits>
	{
		public IslandLimits(int minAgentsInIsland, double minFactorOfAgentsInOtherSkillSet)
		{
			MinAgentsInIsland = minAgentsInIsland;
			MinFactorOfAgentsInOtherSkillSet = minFactorOfAgentsInOtherSkillSet;
		}

		public int MinAgentsInIsland { get; }
		public double MinFactorOfAgentsInOtherSkillSet { get; }

		public int CompareTo(IslandLimits other)
		{
			return MinAgentsInIsland - other.MinAgentsInIsland;
		}
	}
}