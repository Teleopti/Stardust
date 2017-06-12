using Teleopti.Ccc.Domain.InterfaceLegacy;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface INextPlanningPeriodProvider
	{
		IPlanningPeriod Next(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation, IPlanningGroup planningGroup);
		IPlanningPeriod Current(IPlanningGroup planningGroup);
	}
}