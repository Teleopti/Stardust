using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle("Can (probably) not be implemented in teamblock code branch", Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public class IntradayOptimizerLimiter : IIntradayOptimizerLimiter
	{
		private Percent _minPercentOfSkillSetLimit = new Percent(0.5);
		private int _minSizeLimit = 100; 

		public void SetFromTest(Percent sizeOfSkillSetLimit, int minSizeLimit)
		{
			_minPercentOfSkillSetLimit = sizeOfSkillSetLimit;
			_minSizeLimit = minSizeLimit;
		}

		public bool CanJumpOutEarly(int totalNumberOfAgentsInSkillSet, int optimizedNumberOfAgentsInSkillSet)
		{
			return totalNumberOfAgentsInSkillSet >= _minSizeLimit &&
				((double)optimizedNumberOfAgentsInSkillSet/totalNumberOfAgentsInSkillSet) >= _minPercentOfSkillSetLimit.Value;
		}
	}
}