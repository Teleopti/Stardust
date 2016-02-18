namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizerLimiter
	{
		bool CanJumpOutEarly(int sizeOfAgentSkillGroup, int totalNumberOfAgentsInSkillGroup, int optimizedNumberOfAgentsInSkillGroup);
	}
}