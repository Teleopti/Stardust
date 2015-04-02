using Teleopti.Interfaces;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface INextPlanningPeriodProvider
	{
		IPlanningPeriod Current();
	}
}