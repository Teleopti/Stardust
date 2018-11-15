using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface INextPlanningPeriodProvider
	{
		PlanningPeriod Current(PlanningGroup planningGroup);
	}
}