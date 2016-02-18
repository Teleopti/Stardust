namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizerLimiter
	{
		bool CanJumpOutEarly(int totalNumberOfAgentsInSkillGroup, int optimizedNumberOfAgentsInSkillGroup);
	}
}