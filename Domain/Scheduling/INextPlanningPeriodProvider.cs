using Teleopti.Ccc.Domain.InterfaceLegacy;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface INextPlanningPeriodProvider
	{
		IPlanningPeriod Current(IPlanningGroup planningGroup);
	}
}