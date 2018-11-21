using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IPlanningGroupProvider
	{
		PlanningGroup Execute(Guid planningPeriodId);
	}
}