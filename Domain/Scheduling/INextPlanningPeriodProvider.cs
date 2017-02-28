using Teleopti.Ccc.Domain.InterfaceLegacy;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface INextPlanningPeriodProvider
	{
		IPlanningPeriod Next(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation, IAgentGroup agentGroup);
		IPlanningPeriod Current(IAgentGroup agentGroup);
	}
}