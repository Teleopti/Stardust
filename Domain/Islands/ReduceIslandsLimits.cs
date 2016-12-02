namespace Teleopti.Ccc.Domain.Islands
{
	public class ReduceIslandsLimits
	{
		public ReduceIslandsLimits()
		{
			MinimumNumberOfAgentsInIsland = 500;
			MinimumFactorOfAgentsInOtherSkillGroup = 4;
		}

		public int MinimumNumberOfAgentsInIsland { get; private set; }
		public int MinimumFactorOfAgentsInOtherSkillGroup { get; private set; }

		public void SetValues_UseOnlyFromTest(int minAgentsInIsland, int minFactorOfAgentsInOtherSkillGroup)
		{
			MinimumNumberOfAgentsInIsland = minAgentsInIsland;
			MinimumFactorOfAgentsInOtherSkillGroup = minFactorOfAgentsInOtherSkillGroup;
		}
	}
}