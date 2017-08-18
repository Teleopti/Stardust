using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle("Can (probably) not be implemented in teamblock code branch", Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public class IntradayOptimizerLimiter : IIntradayOptimizerLimiter
	{
		private Percent _minPercentOfGroupLimit = new Percent(0.5);
		private int _minSizeLimit = 100; 

		public void SetFromTest(Percent sizeOfGroupLimit, int minSizeLimit)
		{
			_minPercentOfGroupLimit = sizeOfGroupLimit;
			_minSizeLimit = minSizeLimit;
		}

		public bool CanJumpOutEarly(int totalNumberOfAgentsInSkillGroup, int optimizedNumberOfAgentsInSkillGroup)
		{
			return totalNumberOfAgentsInSkillGroup >= _minSizeLimit &&
				((double)optimizedNumberOfAgentsInSkillGroup/totalNumberOfAgentsInSkillGroup) >= _minPercentOfGroupLimit.Value;
		}
	}
}