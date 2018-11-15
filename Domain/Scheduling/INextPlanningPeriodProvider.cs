using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface INextPlanningPeriodProvider
	{
		IPlanningPeriod Current(PlanningGroup planningGroup);
	}
}