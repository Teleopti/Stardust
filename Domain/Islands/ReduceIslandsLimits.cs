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
		public int MinimumFactorOfAgentsInOtherSkillGroup { get; }

		public void SetMinimumNumberOfAgentsInIsland_UseOnlyFromTest(int limit)
		{
			MinimumNumberOfAgentsInIsland = limit;
		}
	}
}