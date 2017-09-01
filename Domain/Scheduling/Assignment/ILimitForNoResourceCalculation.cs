using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public interface ILimitForNoResourceCalculation
	{
		int NumberOfAgents { get; } 
	}
}