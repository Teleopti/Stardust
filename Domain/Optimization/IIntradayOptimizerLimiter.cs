using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle("Can (probably) not be implemented in teamblock code branch", Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public interface IIntradayOptimizerLimiter
	{
		bool CanJumpOutEarly(int totalNumberOfAgentsInSkillSet, int optimizedNumberOfAgentsInSkillSet);
	}
}