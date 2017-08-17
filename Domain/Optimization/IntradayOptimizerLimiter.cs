using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
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