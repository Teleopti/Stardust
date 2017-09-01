using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public class LimitForNoResourceCalculation : ILimitForNoResourceCalculation
	{
		public int NumberOfAgents { get; private set; } = 100;

		public void SetFromTest(int limit)
		{
			NumberOfAgents = limit;
		}
	}
}