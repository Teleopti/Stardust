using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptmizerLimiter : IIntradayOptimizerLimiter
	{
		private Percent _minPercentOfGroupLimit = new Percent(0.5);
		//private int _minSizeLimit = 100; not yet used

		public void SetFromTest(Percent sizeOfGroupLimit, int minSizeLimit)
		{
			_minPercentOfGroupLimit = sizeOfGroupLimit;
			//_minSizeLimit = minSizeLimit;
		}

		public bool CanJumpOutEarly(int sizeOfAgentSkillGroup, int totalNumberOfAgentsInSkillGroup, int optimizedNumberOfAgentsInSkillGroup)
		{
			return (optimizedNumberOfAgentsInSkillGroup/totalNumberOfAgentsInSkillGroup) >= _minPercentOfGroupLimit.Value;
		}
	}
}